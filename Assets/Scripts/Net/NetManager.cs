using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Message;
using UnityEngine;

public class NetManager : SingleMono<NetManager>
{
    public bool IsStart { get; private set; }
    private const int HEART_DELAY = 5000;// ms
    private static IPEndPoint _localIPEndPoint = new(IPAddress.Parse("127.0.0.1"), 0);
    private static IPEndPoint _tcpServerIPEndPoint = new(IPAddress.Parse("127.0.0.1"), 2026);
    private static IPEndPoint _udpServerIPEndPoint = new(IPAddress.Parse("127.0.0.1"), 2027);
    private ConcurrentQueue<NetPackage> _receiveQueue = new();

    private CancellationTokenSource _cancel;
    // Update is called once per frame
    void Update()
    {
        if (!IsStart)
            return;
        while (_receiveQueue.TryDequeue(out var package))
            EventBus.Instance.Trigger<NetPackage>(EventType.OnReceive, package);
    }
    public void StartClient()
    {
        TcpManager.Instance.StartClient(_localIPEndPoint, _tcpServerIPEndPoint);
        var ipEndPoint = TcpManager.Instance.LocalIPEndPoint;
        UdpManager.Instance.StartClient(ipEndPoint, _udpServerIPEndPoint);

        EventBus.Instance.AddListener<NetPackage>(EventType.SendTo, SendToServe);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnNeedResponseMessage);
        _cancel = new();
        Task.Run(TcpHeartLoop);
        IsStart = true;
    }
    public void Close()
    {
        IsStart = false;
        _cancel.Cancel();
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnNeedResponseMessage);
        EventBus.Instance.RemoveListener<NetPackage>(EventType.SendTo, SendToServe);

        TcpManager.Instance.Close();
        UdpManager.Instance.Close();
    }
    public void AddReceivePackage(NetPackage package) => _receiveQueue.Enqueue(package);
    private void SendToServe(NetPackage netPackage)
    {
        if (!IsStart)
        {
            Debug.LogError("【发送失败】客户端网络未启动");
            return;
        }
        if (netPackage.message == null)
            return;
        switch (netPackage.sendType)
        {
            case SendType.Tcp:
                TcpManager.Instance.Send(new TcpPackage(netPackage.message));
                break;
            case SendType.Udp:
                UdpManager.Instance.Send(netPackage);
                break;
        }
    }
    private void OnNeedResponseMessage(NetPackage package)
    {
        if (package.header is UdpHeader udpHeader && udpHeader.IsResponse)
        {
            UdpResponseMessage responseMessage = new();
            responseMessage.PackageId = udpHeader.Id;
            SendToServe(new(new UdpHeader(), responseMessage, SendType.Udp));
            Debug.Log($"【UDP重要消息】PackageID:{udpHeader.Id}");
        }
    }
    private async Task TcpHeartLoop()
    {
        var heartPackage = new TcpPackage(new HeartMessage());
        while (!_cancel.IsCancellationRequested)
        {
            await Task.Delay(HEART_DELAY);
            TcpManager.Instance.Send(heartPackage);
        }
    }
    private void Oestroy()
    {
        Close();
    }
}
