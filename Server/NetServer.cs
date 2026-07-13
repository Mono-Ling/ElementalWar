using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class NetServer
    {
        private Socket _socket;
        private SocketAsyncEventArgs _eventArgs;
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
            TcpClient client = new TcpClient(clientSocket);
            client.StartReveice();

            _eventArgs.AcceptSocket = null;
            _socket.AcceptAsync(args);
        }
    }
}
