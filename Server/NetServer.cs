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
        private static int _playerId;
        private Dictionary<int, TcpClient> _clientDic = new();

        private Socket _socket;
        private SocketAsyncEventArgs _eventArgs;

        private CancellationTokenSource _cancel = new();
        public NetServer(IPEndPoint iPEndPoint,int maxCount)
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Bind(iPEndPoint);
                _socket.Listen(maxCount);
            }
            catch(SocketException e)
            {
                Console.WriteLine("【服务器启动失败】套接字初始化异常" + e);
                return;
            }
            _eventArgs = new SocketAsyncEventArgs();
            _eventArgs.Completed += Accept;
            Console.WriteLine("【服务器启动】");

            Task.Run(ServerLoop);
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnHeartMessage);
        }
        public void Close()
        {
            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnHeartMessage);
            _cancel.Cancel();
            _socket?.Close();
            _eventArgs?.Dispose();

            foreach (var item in _clientDic.Values)
                item.Close();
            _clientDic.Clear();
            Console.WriteLine("【服务器关闭】");
        }
        public void StartAccept() => _socket.AcceptAsync(_eventArgs);
        private void Accept(object? socket,SocketAsyncEventArgs args)
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

            lock(_clientDic)
                _clientDic.Add(id, client);

            _eventArgs.AcceptSocket = null;
            _socket.AcceptAsync(args);
        }
        private void ServerLoop()
        {
            List<int> lostList = new();
            while (!_cancel.IsCancellationRequested)
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
                lock (_clientDic)
                {
                    foreach(int id in lostList)
                        _clientDic.Remove(id);
                }
                lostList.Clear();
            }
            Console.WriteLine("【客户端心跳监测结束】");
        }
        private void OnHeartMessage(ClientPackage package)
        {
            if(package.message is HeartMessage message)
            {
                if (_clientDic.TryGetValue(package.id, out var client))
                {
                    lock (_clientDic)
                        client.preHeartTime = DateTime.Now;
                    Console.WriteLine($"【心跳消息】客户端ID：{package.id}");
                }
            }
        }
    }
}
