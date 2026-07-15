using System;
using System.Collections;
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
    private SocketAsyncEventArgs _sendEventArgs;
    private SocketAsyncEventArgs _receiveEventArgs;
    private byte[] _receiveBuffer = new byte[MAX_SIZE];
    private CancellationTokenSource _cancel;
    public void StartClient(IPEndPoint local, IPEndPoint target)
    {
        _serverIpEndPoint = target;

        _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _sendEventArgs = new();
        _sendEventArgs.Completed += SendCallback;

        _receiveEventArgs = new();
        _receiveEventArgs.SetBuffer(_receiveBuffer, 0, MAX_SIZE);
        _receiveEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _receiveEventArgs.Completed += ReceiveCallback;

        _cancel = new();

        try
        {
            _socket.Bind(local);
            _socket.ReceiveFromAsync(_receiveEventArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError($"【UDP客户端启动失败】{e.Message}");
            return;
        }

        IsStart = true;

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
        _sendEventArgs?.Dispose();
    }
    public void Send(NetPackage netPackage)
    {
        if (!IsStart)
            return;
        if (netPackage.message == null || netPackage.header is not UdpHeader header)
        {
            Debug.LogWarning("【UDP发送失败】消息为空");
            return;
        }
        header.Time = DateTime.UtcNow.Ticks;
        header.Type = netPackage.message.GetType().ToString();
        uint packageId = (uint)Interlocked.Increment(ref _packageId) - 1;
        header.Id = packageId;

        UdpPackage udpPackage = new(header, netPackage.message);
        SendToTarget(_serverIpEndPoint, udpPackage);

        if (header.IsResponse)
            lock (_overSendPackageDic)
                _overSendPackageDic.Add(packageId, (udpPackage, DEFAULT_OVER_SEND_TIMES));
    }
    private async void OverSendLoop()
    {
        List<uint> lostPackageList = new();
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
                        _overSendPackageDic[item.Key] = (package, times);
                    }
                    foreach (uint id in lostPackageList)
                        _overSendPackageDic.Remove(id);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"【UDP超时重传异常】{e.Message}");
            }
            lostPackageList.Clear();
            await Task.Delay(OVER_SEND_DELAY).ConfigureAwait(true);
        }
    }
    private void SendToTarget(IPEndPoint target, UdpPackage package)
    {
        if (!IsStart)
            return;
        byte[] bytes = package.data;
        if (bytes == null)
            return;
        try
        {
            _sendEventArgs.RemoteEndPoint = target;
            _sendEventArgs.SetBuffer(bytes, 0, bytes.Length);
            _socket.SendToAsync(_sendEventArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError("【UDP发送失败】" + e);
        }
    }
    private void SendCallback(object socket, SocketAsyncEventArgs args)
    {
        if (!IsStart)
            return;
        if (args.SocketError != SocketError.Success)
        {
            Debug.LogError("【UDP发送失败】" + args.SocketError);
            return;
        }
        Debug.Log($"【UDP发送成功】Target：{args.RemoteEndPoint}");
    }
    private void ReceiveCallback(object socketObj, SocketAsyncEventArgs args)
    {
        if (!IsStart)
            return;
        if (args.SocketError != SocketError.Success)
        {
            Debug.LogError("【UDP接收失败】" + args.SocketError);
            return;
        }
        if ((args.RemoteEndPoint as IPEndPoint).Equals(_serverIpEndPoint))
        {
            int length = args.BytesTransferred;
            if (length == 0)
            {
                _socket?.ReceiveFromAsync(_receiveEventArgs);
                Debug.LogWarning("【UDP消息】无效数据包");
                return;
            }
            byte[] bytes = new byte[length];
            Array.Copy(args.Buffer, 0, bytes, 0, length);

            UdpPackage udpPackage = new(bytes);
            if (udpPackage.header == null || udpPackage.message == null)
            {
                _socket?.ReceiveFromAsync(_receiveEventArgs);
                Debug.LogError("【UDP消息解析失败】");
                return;
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

        if (socketObj is Socket socket)
            socket.ReceiveFromAsync(args);
        else
            Debug.LogError("【UDP接收重启失败】");
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
