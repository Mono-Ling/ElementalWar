using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using Message;
using Server.Event;

namespace Server
{
    internal class TcpClient
    {
        public bool IsConnected { get; private set; }
        public DateTime preHeartTime;
        private int _id;
        private const int MAX_PACKAGE_SIZE = 1024;
        private Socket _socket;
        private SocketAsyncEventArgs _sendEventArgs = new();
        private SocketAsyncEventArgs _receiveEventArgs = new();
        private byte[] _receiveBuffer = new byte[MAX_PACKAGE_SIZE];

        private TcpPackage? _bufferPackage;
        public TcpClient(Socket socket,int id)
        {
            if(socket == null || !socket.Connected)
            {
                Console.WriteLine("【客户端连接失败】无效Socket");
                return;
            }

            this._id = id;
            _socket = socket;
            preHeartTime = DateTime.Now;

            _receiveEventArgs.SetBuffer(_receiveBuffer, 0, MAX_PACKAGE_SIZE);
            _receiveEventArgs.Completed += ReceiveCallback;

            _sendEventArgs.Completed += SendCallback;

            IsConnected = true;

            Console.WriteLine($"【客户端接入成功】客户端：{_socket.RemoteEndPoint}");

            TextMessage text = new();
            text.Content = $"客户端{_socket.RemoteEndPoint}欢迎接入服务器";
            Send(new TcpPackage(text));
        }
        public void StartReveice()
        {
            if (_socket == null || !IsConnected) return;
            try
            {
                _socket.ReceiveAsync(_receiveEventArgs);
            }
            catch (SocketException e)
            {
                Console.WriteLine("【客户端接收启动异常】" + e.SocketErrorCode);
            }
        }
        public void Send(TcpPackage package)
        {
            if (!IsConnected) return;
            if (package == null)
            {
                Console.WriteLine("【无效数据】字节数组为空");
                return;
            }
            byte[] bytes = package.data;
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
            IsConnected = false;
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
            if(!IsConnected) return;
            if (args.SocketError != SocketError.Success)
            {
                Console.WriteLine($"【发送失败】客户端：{_socket?.RemoteEndPoint}错误代码：{args?.SocketError}");
                return;
            }
            Console.WriteLine($"【发送成功】Target：{_socket?.RemoteEndPoint}");
        }
        private void ReceiveCallback(object? socket, SocketAsyncEventArgs args)
        {
            if (!IsConnected) return;
            if (args.SocketError != SocketError.Success)
            {
                try
                {
                    var endpoint = _socket.RemoteEndPoint;
                    Console.WriteLine($"【接收失败】客户端：{endpoint} 错误代码：{args.SocketError}");
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine($"【接收失败】Socket已释放，错误代码：{args.SocketError}");
                }
                Close();
                return;
            }
            byte[]? message = args.Buffer;
            int messageSize = args.BytesTransferred;
            if(messageSize == 0)
            {
                Close();
                return;
            }
            ProcessReceive(message,messageSize);
            _socket.ReceiveAsync(_receiveEventArgs);
        }
        private void ProcessReceive(byte[]? bytes,int length)
        {
            if (bytes == null || bytes.Length == 0)
                return;
            int offset = 0;
            while (offset < length)
            {
                if(_bufferPackage == null)
                    _bufferPackage = new TcpPackage(bytes,ref offset);        
                else
                    _bufferPackage.Append(bytes, ref offset);

                if (_bufferPackage.IsCompleted)
                {
                    EventBus.Instance.Trigger<ClientPackage>(EventType.OnReceive,new(_id,_bufferPackage.message));
                    _bufferPackage = null;
                }
            }
        }
    }
}
