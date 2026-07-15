using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Message;
using Server.Event;

namespace Server
{
    internal class UdpServer
    {
        private const int DEFAULT_OVER_SEND_TIMES = 5;
        private const int OVER_SEND_DELAY = 500;// ms

        private const int CLEAR_HISTORY_PACKAGE_DELAY = 5000;// ms
        private const float HISTORY_PACKAGE_WINDOW = 3f;// s

        private static uint _packageId;
        public bool IsStart { get; private set; }
        private const int MAX_SIZE = 1024;
        /// <summary>
        /// key -> playerId
        /// value -> playerIP
        /// </summary>
        private Dictionary<int, IPEndPoint>? _sendDic;
        /// <summary>
        /// key -> playerIP
        /// value -> playerId
        /// </summary>
        private Dictionary<IPEndPoint,int>? _receiveDic;

        /// <summary>
        /// key -> 包序号
        /// value -> (重要包，Target，剩余重传次数）
        /// </summary>
        private Dictionary<uint, (UdpPackage package,IPEndPoint target,int times)> _overSendPackageDic = new();
        /// <summary>
        /// 已接收包序号字典
        /// key -> packageId
        /// value -> time
        /// </summary>
        private Dictionary<uint,long> _historyPackageDic = new();

        private Socket? _socket;
        private SocketAsyncEventArgs _sendEventArgs;
        private SocketAsyncEventArgs? _receiveEventArgs;
        private byte[] _receiveBuffer = new byte[MAX_SIZE];

        private ConcurrentQueue<ClientPackage> _sendQueue = new();

        private CancellationTokenSource _cancel;
        public UdpServer(Dictionary<int, IPEndPoint>? sendDic, Dictionary<IPEndPoint, int>? receiveDic)
        {
            if(sendDic == null || receiveDic == null)
            {
                Console.WriteLine("【UDP服务器初始化失败】发送接收映射表为空");
                return;
            }
            _sendDic = sendDic;
            _receiveDic = receiveDic;
        }
        public void StartUdpServer(IPEndPoint local)
        {
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
                Console.WriteLine($"【UDP服务器启动失败】{e.Message}");
                return;
            }

            Task.Run(SendLoop);
            Task.Run(OverSend);
            Task.Run(ClearHistoryPackage);
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnUdpResponseMessage);

            IsStart = true;
        }
        public void Close()
        {
            IsStart = false;

            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
            }
            catch { }

            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnUdpResponseMessage);

            _cancel?.Cancel();

            _socket?.Close();
            _socket?.Dispose();
            _receiveEventArgs?.Dispose();
            _sendEventArgs?.Dispose();
            Console.WriteLine("【UDP服务器关闭】");
        }
        public void Send(ClientPackage package) => _sendQueue.Enqueue(package);
        private void SendLoop()
        {
            while(!_cancel.IsCancellationRequested)
            {
                while(_sendQueue.TryDequeue(out var clientPackage))
                {
                    if (clientPackage.header == null || clientPackage.message == null)
                        continue;
                    if(_sendDic.TryGetValue(clientPackage.playerId,out var target) && clientPackage.header is UdpHeader header)
                    {
                        header.Time = DateTime.UtcNow.Ticks;
                        header.Type = clientPackage.message.GetType().ToString();
                        uint packageId = _packageId++;
                        header.Id = packageId;

                        UdpPackage package = new(header, clientPackage.message);
                        SendToTarget(target, package);

                        if (header.IsResponse)
                            lock(_overSendPackageDic)
                                _overSendPackageDic.Add(packageId, (package, target, DEFAULT_OVER_SEND_TIMES));
                    }
                }
            }
        }
        private async Task OverSend()
        {
            List<uint> lostPackageList = new();
            while(!_cancel.IsCancellationRequested)
            {
                await Task.Delay(OVER_SEND_DELAY);
                lock (_overSendPackageDic)
                {
                    foreach (var item in _overSendPackageDic)
                    {
                        (UdpPackage package, IPEndPoint target, int times) = item.Value;

                        SendToTarget(target, package);
                        times--;
                        if (times == 0)
                        {
                            lostPackageList.Add(item.Key);
                            continue;
                        }
                        _overSendPackageDic[item.Key] = (package, target, times);
                    }
                    foreach (uint item in lostPackageList)
                        _overSendPackageDic.Remove(item);
                }
                lostPackageList.Clear();
            }
        }
        private void SendToTarget(IPEndPoint target,UdpPackage package)
        {
            if (!IsStart)
                return;
            byte[]? bytes = package.data;
            if (bytes == null)
                return;
            try
            {
                _sendEventArgs.RemoteEndPoint = target;
                _sendEventArgs.SetBuffer(bytes, 0, bytes.Length);
                _socket?.SendToAsync(_sendEventArgs);
            }
            catch (SocketException e)
            {
                Console.WriteLine("【UDP发送失败】" + e);
            }
        }
        private void SendCallback(object? socketObj, SocketAsyncEventArgs args)
        {
            if (!IsStart)
                return;
            if(args.SocketError != SocketError.Success)
            {
                Console.WriteLine("【UDP发送失败】" + args.SocketError);
                return;
            }
            Console.WriteLine($"【UDP发送成功】Target：{args.RemoteEndPoint}");
        }
        private void ReceiveCallback(object? socketObj, SocketAsyncEventArgs args)
        {
            if (!IsStart)
                return;
            if(args.SocketError != SocketError.Success)
            {
                Console.WriteLine("【UDP接收失败】" + args.SocketError);
                return;
            }
            if(_receiveDic.TryGetValue(args.RemoteEndPoint as IPEndPoint,out int id))
            {
                int length = args.BytesTransferred;
                if (length == 0)
                {
                    _socket?.ReceiveFromAsync(_receiveEventArgs);
                    return;
                }
                byte[] bytes = new byte[length];
                Array.Copy(args.Buffer,0, bytes, 0, length);

                UdpPackage udpPackage = new(bytes);
                lock (_historyPackageDic)
                {
                    if (!_historyPackageDic.ContainsKey(udpPackage.header.Id))
                    {
                        _historyPackageDic.Add(udpPackage.header.Id, udpPackage.header.Time);

                        ClientPackage clientPackage = new(id, udpPackage.header, udpPackage.message, SendType.Udp);
                        EventBus.Instance.Trigger<ClientPackage>(EventType.OnReceive, clientPackage);
                    }
                    else
                        Console.Write($"【UDP重复消息】PackageId:{udpPackage.header.Id}");
                }
            }
            else
                Console.WriteLine($"【UDP未知消息源】From：{args.RemoteEndPoint}");

            if (socketObj is Socket socket)
                socket.ReceiveFromAsync(args);
            else
                Console.WriteLine("【UDP接收启动失败】");
        }
        private async Task ClearHistoryPackage()
        {
            List<uint> lostPackageList = new();
            while(!_cancel.IsCancellationRequested)
            {
                await Task.Delay(CLEAR_HISTORY_PACKAGE_DELAY);

                lock(_historyPackageDic)
                {
                    foreach(var item in _historyPackageDic)
                    {
                        DateTime packaeTime = new(item.Value);
                        if((DateTime.UtcNow - packaeTime).TotalSeconds > HISTORY_PACKAGE_WINDOW)
                            lostPackageList.Add(item.Key);
                    }
                    foreach(uint id in lostPackageList)
                        _historyPackageDic.Remove(id);
                }
                lostPackageList.Clear();
            }
        }
        private void OnUdpResponseMessage(ClientPackage clientPackage)
        {
            if (clientPackage.message is UdpResponseMessage udpResponseMessage)
                lock (_overSendPackageDic)
                    _overSendPackageDic.Remove(udpResponseMessage.PackageId);
        }
    }
}
