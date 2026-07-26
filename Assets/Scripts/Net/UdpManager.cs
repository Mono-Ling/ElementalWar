using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Message;
using UnityEngine;

public class UdpManager : SingleMono<UdpManager>
{
    public IPEndPoint LocalIPEndPoint => _socket.LocalEndPoint as IPEndPoint;
    private const int DEFAULT_OVER_SEND_TIMES = 5;
    private const int OVER_SEND_DELAY = 500;// ms

    private const int CLEAR_HISTORY_PACKAGE_DELAY = 5000;// ms
    private const float HISTORY_PACKAGE_WINDOW = 3f;// s

    private IPEndPoint _serverIpEndPoint;

    private static int _packageId;
    public bool IsStart { get; private set; }
    private const int MAX_SIZE = 1024;

    /// <summary>
    /// key -> 包序号
    /// value -> (重要包，剩余重传次数）
    /// </summary>
    private Dictionary<uint, (UdpPackage package, int times)> _overSendPackageDic = new();
    /// <summary>
    /// 已接收包序号字典
    /// key -> packageId
    /// value -> time
    /// </summary>
    private Dictionary<uint, long> _historyPackageDic = new();

    private Socket _socket;
    private SocketAsyncEventArgs _receiveEventArgs;
    private byte[] _receiveBuffer = new byte[MAX_SIZE];

    private ConcurrentQueue<NetPackage> _sendQueue = new();

    private CancellationTokenSource _cancel;
    public void StartClient(IPEndPoint local, IPEndPoint target)
    {
        _serverIpEndPoint = target;

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        _receiveEventArgs = new();
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, MAX_SIZE);
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.Completed += ReceiveCallback;

        _cancel = new();

        try
        {
            _socket.Bind(local);
            IsStart = true;
            // 检查同步完成：若 ReceiveFromAsync 返回 false，需要手动调用回调
            if (!_socket.ReceiveFromAsync(_receiveEventArgs))
                ReceiveCallback(_socket, _receiveEventArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError($"【UDP客户端启动失败】{e.Message}");
            return;
        }

        Task.Run(SendLoop);
        Task.Run(OverSendLoop);
        Task.Run(ClearHistoryPackageLoop);

        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnUdpResponseMessage);
    }
    public void Close()
    {
        IsStart = false;

        try
        {
            _socket?.Shutdown(SocketShutdown.Both);
        }
        catch { }

        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnUdpResponseMessage);

        _cancel?.Cancel();

        _socket?.Close();
        _socket?.Dispose();
        _receiveEventArgs?.Dispose();
    }
    public void Send(NetPackage netPackage)
    {
        if (!IsStart)
            return;
        if (netPackage.message == null || netPackage.header is not UdpHeader)
        {
            Debug.LogWarning("【UDP发送失败】消息为空");
            return;
        }
        _sendQueue.Enqueue(netPackage);
    }
    private void SendLoop()
    {
        while (!_cancel.IsCancellationRequested)
        {
            try
            {
                while (_sendQueue.TryDequeue(out var netPackage))
                {
                    if (netPackage.message == null || netPackage.header is not UdpHeader header)
                        continue;

                    header.Time = DateTime.UtcNow.Ticks;
                    header.Type = netPackage.message.GetType().ToString();
                    uint packageId = (uint)Interlocked.Increment(ref _packageId) - 1;
                    header.Id = packageId;

                    UdpPackage udpPackage = new(header, netPackage.message);
                    // 使用同步 SendTo，避免 _sendEventArgs 重用冲突
                    // UDP SendTo 是非阻塞的，仅拷贝到内核缓冲区
                    _socket.SendTo(udpPackage.data, _serverIpEndPoint);

                    if (header.IsResponse)
                        lock (_overSendPackageDic)
                            _overSendPackageDic.Add(packageId, (udpPackage, DEFAULT_OVER_SEND_TIMES));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"【UDP发送循环异常】{e.Message}");
            }
            // 避免队列空时 100% CPU 忙等
            Thread.Sleep(1);
        }
    }
    private async Task OverSendLoop()
    {
        List<uint> lostPackageList = new();
        List<(uint key, UdpPackage package, int times)> updateList = new();
        await Task.Delay(OVER_SEND_DELAY).ConfigureAwait(true);
        while (!_cancel.IsCancellationRequested)
        {
            try
            {
                lock (_overSendPackageDic)
                {
                    foreach (var item in _overSendPackageDic)
                    {
                        (var package, int times) = item.Value;

                        _socket.SendTo(package.data, _serverIpEndPoint);
                        times--;
                        if (times == 0)
                        {
                            lostPackageList.Add(item.Key);
                            continue;
                        }
                        updateList.Add((item.Key, package, times));
                    }
                    foreach (uint id in lostPackageList)
                        _overSendPackageDic.Remove(id);
                    foreach (var (key, package, times) in updateList)
                        _overSendPackageDic[key] = (package, times);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"【UDP超时重传异常】{e.Message}");
            }
            lostPackageList.Clear();
            updateList.Clear();
            await Task.Delay(OVER_SEND_DELAY).ConfigureAwait(true);
        }
    }
    private void ReceiveCallback(object socketObj, SocketAsyncEventArgs args)
    {
        // 接收链保护：所有分支不要独立重启接收，统一在末尾重启，
        // 避免在同一回调内并发启动多个异步操作导致 SocketAsyncEventArgs 冲突。
        if (!IsStart)
            goto exit;
        if (args.SocketError != SocketError.Success)
        {
            if (args.SocketError == SocketError.ConnectionReset)
            {
                Debug.LogWarning("【UDP接收】ConnectionReset（忽略），重置远端");
                args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            }
            else
                Debug.LogError("【UDP接收失败】" + args.SocketError);
            goto exit;
        }
        if (args.RemoteEndPoint is not IPEndPoint remoteEp)
            goto exit;
        if (remoteEp.Equals(_serverIpEndPoint))
        {
            int length = args.BytesTransferred;
            if (length == 0)
            {
                Debug.LogWarning("【UDP消息】无效数据包");
                goto exit;
            }
            byte[] bytes = new byte[length];
            Array.Copy(args.Buffer, 0, bytes, 0, length);

            UdpPackage udpPackage = new(bytes);
            if (udpPackage.header == null || udpPackage.message == null)
            {
                Debug.LogError("【UDP消息解析失败】");
                goto exit;
            }
            bool isNewPackage = false;
            lock (_historyPackageDic)
            {
                if (!_historyPackageDic.ContainsKey(udpPackage.header.Id))
                {
                    _historyPackageDic.Add(udpPackage.header.Id, udpPackage.header.Time);
                    isNewPackage = true;
                }
            }
            if (isNewPackage)
            {
                NetPackage netPackage = new(udpPackage.header, udpPackage.message, SendType.Udp);
                NetManager.Instance.AddReceivePackage(netPackage);
                Debug.Log("【服务器UDP消息】");
            }
            else
                Debug.LogWarning($"【UDP重复消息】PackageId:{udpPackage.header.Id}");
        }
        else
            Debug.LogWarning($"【UDP未知消息源】From：{args.RemoteEndPoint}");

    exit:
        RestartReceive(args);
    }
    /// <summary>
    /// 重新挂载异步接收。在 Completed 回调内部调用是安全的。
    /// 若同步完成则提交到线程池处理，避免调用栈递归过深或重入冲突。
    /// </summary>
    private void RestartReceive(SocketAsyncEventArgs args)
    {
        if (!IsStart || _socket == null)
            return;
        try
        {
            if (!_socket.ReceiveFromAsync(args))
            {
                // 同步完成 → 不在当前回调栈内递归，提交线程池处理
                ThreadPool.QueueUserWorkItem(_ => ReceiveCallback(_socket, args));
            }
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException)
        {
            // 极少数并发竞态导致 args 仍在挂起中，忽略；
            // 下一个完成的回调会再次调用 RestartReceive
        }
        catch (SocketException e)
        {
            Debug.LogError("【UDP接收重启失败】" + e.SocketErrorCode);
        }
    }
    private async Task ClearHistoryPackageLoop()
    {
        List<uint> lostPackageList = new();
        while (!_cancel.IsCancellationRequested)
        {
            await Task.Delay(CLEAR_HISTORY_PACKAGE_DELAY).ConfigureAwait(true);

            try
            {
                lock (_historyPackageDic)
                {
                    foreach (var item in _historyPackageDic)
                    {
                        DateTime packageTime = new(item.Value);
                        if ((DateTime.UtcNow - packageTime).TotalSeconds > HISTORY_PACKAGE_WINDOW)
                            lostPackageList.Add(item.Key);
                    }
                    foreach (uint id in lostPackageList)
                        _historyPackageDic.Remove(id);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"【UDP历史包清除异常】{e.Message}");
            }
            lostPackageList.Clear();
        }
    }
    private void OnUdpResponseMessage(NetPackage package)
    {
        if (package.message is UdpResponseMessage udpResponse)
        {
            lock (_overSendPackageDic)
                _overSendPackageDic.Remove(udpResponse.PackageId);
            Debug.Log($"【UDP回复消息】package:{udpResponse.PackageId}");
        }
    }
}
