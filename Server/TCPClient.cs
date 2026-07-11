using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class TCPClient
    {
        private const int MAX_PACKAGE_SIZE = 1024;
        private Socket _socket;
        private SocketAsyncEventArgs _sendEventArgs = new();
        private SocketAsyncEventArgs _receiveEventArgs = new();
        private byte[] _receiveBuffer = new byte[MAX_PACKAGE_SIZE];
        private byte[] _tempBytes = new byte[MAX_PACKAGE_SIZE];
        private int _tempBytesLength = 0;
        public TCPClient(Socket socket)
        {
            if(socket == null || !socket.Connected)
            {
                Console.WriteLine("【客户端连接失败】无效Socket");
                return;
            }
            _socket = socket;

            _receiveEventArgs.SetBuffer(_receiveBuffer, 0, MAX_PACKAGE_SIZE);
            _receiveEventArgs.Completed += ReceiveCallback;

            _sendEventArgs.Completed += SendCallback;
            Console.WriteLine($"【客户端接入成功】客户端：{_socket.RemoteEndPoint}");
        }
        public void StartReveice()
        {
            if (_socket == null) return;
            try
            {
                _socket.ReceiveAsync(_receiveEventArgs);
            }
            catch (SocketException e)
            {
                Console.WriteLine("【客户端接收启动异常】" + e.SocketErrorCode);
            }
        }
        public void Send(byte[] bytes)
        {
            if (bytes == null)
            {
                Console.WriteLine("【无效数据】字节数组为空");
                return;
            }

            try
            {
                _sendEventArgs.SetBuffer(bytes, 0, bytes.Length);
                _socket.SendAsync(_sendEventArgs);
            }
            catch (SocketException e)
            {
                Console.WriteLine("【发送失败】错误代码" + e);
            }
        }
        public void Close()
        {
            try
            {
                Console.WriteLine($"【客户端断连】客户端：{_socket?.RemoteEndPoint}连接断开");
                _socket?.Shutdown(SocketShutdown.Both);
            }
            catch { }

            _socket?.Close();
            _socket?.Dispose();
            _sendEventArgs?.Dispose();
            _receiveEventArgs?.Dispose();
        }
        private void SendCallback(object? socket,SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                Console.WriteLine($"【发送失败】客户端：{_socket?.RemoteEndPoint}错误代码：{args?.SocketError}");
                return;
            }
            Console.WriteLine($"【发送成功】Target：{_socket?.RemoteEndPoint}");
        }
        private void ReceiveCallback(object? socket, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                Console.WriteLine($"【接收失败】客户端：{_socket?.RemoteEndPoint}错误代码：{args?.SocketError}");
                return;
            }
            byte[] message = args.Buffer;
            int messageSize = args.BytesTransferred;
            if(messageSize == 0)
            {
                Close();
                return;
            }
            _socket.ReceiveAsync(_receiveEventArgs);
        }
    }
}
