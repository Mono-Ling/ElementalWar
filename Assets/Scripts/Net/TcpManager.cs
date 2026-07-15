using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using UnityEngine;

public class TcpManager : SingleMono<TcpManager>
{
    public bool IsStart { get; private set; }
    public IPEndPoint LocalIPEndPoint => _socket.LocalEndPoint as IPEndPoint;
    private const int MAX_SIZE = 1024;
    private Socket _socket;
    private SocketAsyncEventArgs _connectArgs;
    private SocketAsyncEventArgs _sendArgs;
    private SocketAsyncEventArgs _receiveArgs;
    private byte[] _receiveBuffer = new byte[MAX_SIZE];
    private TcpPackage _bufferPackage;
    public void StartClient(IPEndPoint local, IPEndPoint target)
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
            _socket.Bind(local);

            _connectArgs.RemoteEndPoint = target;
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
    public void Send(TcpPackage package)
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
                NetPackage netPackage = new(_bufferPackage.message, SendType.Tcp);
                NetManager.Instance.AddReceivePackage(netPackage);
                _bufferPackage = null;
            }
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
