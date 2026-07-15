using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Message;
using Server.Event;

namespace Server
{
    internal class NetServer
    {
        private const float MAX_OUTTIME = 10;
        private const int CLEAR_OVERTIME_DELAY = 1000;// ms

        private static int _playerId;

        private object _lockObj = new();
        private Dictionary<int, TcpClient> _clientDic = new();

        private Dictionary<int, IPEndPoint> _udpSendDic = new();
        private Dictionary<IPEndPoint,int> _udpReceiveDic = new();

        private Socket _socket;
        private SocketAsyncEventArgs _eventArgs;

        private CancellationTokenSource _cancel = new();

        private UdpServer _udpServer;
        public NetServer()
        {
            _udpServer = new(_udpSendDic, _udpReceiveDic);
        }
        public void Start(IPEndPoint tcpIPEndPoint, IPEndPoint udpIPEndPoint, int maxCount = 100)
        {
            _cancel = new();

            _eventArgs = new SocketAsyncEventArgs();
            _eventArgs.Completed += AcceptCallback;

            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Bind(tcpIPEndPoint);
                _socket.Listen(maxCount);
                _socket.AcceptAsync(_eventArgs);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"【服务器长连接启动失败】{e.Message}");
                return;
            }

            _udpServer.StartUdpServer(udpIPEndPoint);

            Task.Run(ClearOverTimeLoop);
            EventBus.Instance.AddListener<ClientPackage>(EventType.SendTo,SendTo);
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnHeartMessage);
            Console.WriteLine("【服务器启动】");
        }
        public void Close()
        {
            EventBus.Instance.RemoveListener<ClientPackage>(EventType.SendTo, SendTo);
            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnHeartMessage);
            _cancel.Cancel();
            _socket?.Close();
            _socket?.Dispose();
            _eventArgs?.Dispose();

            _udpServer.Close();

            foreach (var item in _clientDic.Values)
                item.Close();
            _clientDic.Clear();

            _udpSendDic?.Clear();
            _udpReceiveDic?.Clear();

            Console.WriteLine("【服务器关闭】");
        }
        private void SendTo(ClientPackage clientPackage)
        {
            if (clientPackage.message == null)
                return;
            if (_clientDic.TryGetValue(clientPackage.playerId, out var item))
            {
                switch (clientPackage.sendType)
                {
                    case SendType.Tcp:
                        item.Send(new TcpPackage(clientPackage.message));
                        break;
                    case SendType.Udp:
                        _udpServer.Send(clientPackage);
                        break;
                }
            }
            else
                Console.WriteLine($"【找不到发送对象】TargetID:{clientPackage.playerId}");
        }
        private void AcceptCallback(object? socket,SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                (socket as Socket)?.AcceptAsync(args);
                return;
            }
            Socket clientSocket = args.AcceptSocket;
            int id = _playerId++;
            TcpClient client = new TcpClient(clientSocket,id);
            client.StartReveice();

            lock (_lockObj)
            {
                _clientDic.Add(id, client);

                IPEndPoint remote = clientSocket.RemoteEndPoint as IPEndPoint;
                remote = new(remote.Address,remote.Port);
                _udpSendDic.Add(id, remote);
                _udpReceiveDic.Add(remote, id);
            }

            _eventArgs.AcceptSocket = null;
            _socket.AcceptAsync(args);
        }
        private async Task ClearOverTimeLoop()
        {
            List<int> lostList = new();
            while (!_cancel.IsCancellationRequested)
            {
                await Task.Delay(CLEAR_OVERTIME_DELAY);
                lock (_lockObj)
                {
                    foreach (var client in _clientDic)
                    {
                        if ((DateTime.Now - client.Value.preHeartTime).TotalSeconds > MAX_OUTTIME)
                        {
                            lostList.Add(client.Key);
                            Console.WriteLine($"【客户端超时】客户端ID：{client.Key}");
                            client.Value.Close();
                        }
                    }

                    foreach (int id in lostList)
                    {
                        _clientDic.Remove(id);

                        _udpReceiveDic.Remove(_udpSendDic[id]);
                        _udpSendDic.Remove(id);
                    }
                }
                lostList.Clear();
            }
            Console.WriteLine("【客户端心跳监测结束】");
        }
        private void OnHeartMessage(ClientPackage package)
        {
            if(package.message is HeartMessage message)
            {
                if (_clientDic.TryGetValue(package.playerId, out var client))
                {
                    lock (_clientDic)
                        client.preHeartTime = DateTime.Now;
                    Console.WriteLine($"【心跳消息】客户端ID：{package.playerId}");
                }
            }
        }
    }
}
