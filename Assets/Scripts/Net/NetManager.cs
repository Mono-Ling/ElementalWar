using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using UnityEngine;

public class NetManager : SingleMono<NetManager>
{
    public bool IsStart { get; private set; }
    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 2026;
    private const int MAX_SIZE = 1024;
    private IPEndPoint _serverIPEndPoint = new(IPAddress.Parse(SERVER_IP), SERVER_PORT);
    private Socket _socket;
    private SocketAsyncEventArgs _connectArgs;
    private SocketAsyncEventArgs _sendArgs;
    private SocketAsyncEventArgs _receiveArgs;
    private byte[] _receiveBuffer = new byte[MAX_SIZE];
    private TcpPackage _bufferPackage;

    private ConcurrentQueue<TcpPackage> _sendQueue = new();
    private ConcurrentQueue<TcpPackage> _receiveQueue = new();
    void Update()
    {
        if (!IsStart)
            return;

        while (_sendQueue.TryDequeue(out TcpPackage package))
            SendToServer(package);

        while (_receiveQueue.TryDequeue(out TcpPackage package))
            EventBus.Instance.Trigger<IMessage>(EventType.OnReceive, package.message);
    }
    public void StartClient()
    {
        _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        _connectArgs = new();
        _connectArgs.Completed += ConnectCallback;

        _sendArgs = new();
        _sendArgs.Completed += SendCallback;

        _receiveArgs = new();
        _receiveArgs.Completed += ReceiveCallback;
        _receiveArgs.SetBuffer(_receiveBuffer, 0, MAX_SIZE);

        try
        {
            _connectArgs.RemoteEndPoint = _serverIPEndPoint;
            _socket.ConnectAsync(_connectArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError("【连接失败】" + e);
            Close();
            return;
        }

        IsStart = true;
        Debug.Log("【客户端启动】");
    }
    public void Close()
    {
        IsStart = false;
        try
        {
            Debug.LogWarning($"【断连】服务器：{_socket?.RemoteEndPoint}连接断开");
            _socket?.Shutdown(SocketShutdown.Both);
        }
        catch { }

        _socket?.Close();
        _socket?.Dispose();
        _sendArgs?.Dispose();
        _receiveArgs?.Dispose();
    }
    public void Send(TcpPackage package)
    {
        if (package == null)
            return;
        _sendQueue.Enqueue(package);
    }
    private void StartReceive()
    {
        if (_socket == null) return;
        try
        {
            _socket.ReceiveAsync(_receiveArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError("【消息接收启动异常】" + e.SocketErrorCode);
            Close();
            return;
        }
        Debug.Log("【启动TCP消息接收】");
    }
    private void SendToServer(TcpPackage package)
    {
        if (package == null)
            return;
        if (!IsStart)
        {
            Debug.LogWarning("【消息发送失败】连接断开");
            return;
        }
        byte[] bytes = package.data;
        try
        {
            _sendArgs.SetBuffer(bytes, 0, bytes.Length);
            _socket.SendAsync(_sendArgs);
        }
        catch (SocketException e)
        {
            Debug.LogError("【消息发送失败】" + e);
        }
    }
    private void ConnectCallback(object socket, SocketAsyncEventArgs args)
    {
        if (args.SocketError != SocketError.Success)
        {
            Debug.LogError("【连接失败】" + args.SocketError);
            return;
        }
        StartReceive();
        Debug.Log($"【连接成功】Target:{_socket?.RemoteEndPoint}");
    }
    private void SendCallback(object socket, SocketAsyncEventArgs args)
    {
        if (!IsStart) return;
        if (args.SocketError != SocketError.Success)
        {
            Debug.LogError("【消息发送失败】" + args.SocketError);
            return;
        }
        Debug.Log($"【消息发送成功】Target：{_socket.RemoteEndPoint}");
    }
    private void ReceiveCallback(object socket, SocketAsyncEventArgs args)
    {
        if (!IsStart) return;
        if (args.SocketError != SocketError.Success)
        {
            Debug.LogError("【消息接收失败】" + args.SocketError);
            return;
        }
        byte[] bytes = args.Buffer;
        int length = args.BytesTransferred;
        if (length == 0)
        {
            Close();
            return;
        }
        ProcessReceive(bytes, length);
        (socket as Socket).ReceiveAsync(_receiveArgs);
    }
    private void ProcessReceive(byte[] bytes, int length)
    {
        if (bytes == null || bytes.Length == 0)
            return;
        int offset = 0;
        while (offset < length)
        {
            if (_bufferPackage == null)
                _bufferPackage = new TcpPackage(bytes, ref offset);
            else
                _bufferPackage.Append(bytes, ref offset);

            if (_bufferPackage.IsCompleted)
            {
                //EventBus.Instance.Trigger<IMessage>(EventType.OnReceive, _bufferPackage.message);
                _receiveQueue.Enqueue(_bufferPackage);
                _bufferPackage = null;
            }
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
