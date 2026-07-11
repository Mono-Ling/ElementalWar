using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Server
    {
        private Socket _socket;
        private SocketAsyncEventArgs _eventArgs;
        public Server(IPEndPoint iPEndPoint,int maxCount)
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(iPEndPoint);
            _socket.Listen(maxCount);
            _eventArgs = new SocketAsyncEventArgs();
            _eventArgs.Completed += Accept;
        }
        private void Accept(object? socket,SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                (socket as Socket)?.AcceptAsync(args);
                return;
            }
            Socket? client = args.AcceptSocket;

            (socket as Socket)?.AcceptAsync(args);
        }
    }
}
