using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Google.Protobuf;
using Message;
using UnityEngine;

[System.Serializable]
public abstract class BaseSynSend
{
    protected Blackboard blackboard;
    protected UdpHeader udpHeader;
    protected void SetHeader(bool isResponse = false)
    => udpHeader = new() { IsResponse = isResponse };
    protected void Send(IMessage message)
    {
        if (udpHeader == null || message == null)
        {
            Debug.LogWarning("【状态同步发送】无效消息体");
            return;
        }
        NetPackage package = new(udpHeader, message);
        EventBus.Instance.Trigger(EventType.SendTo, package);
    }
    public virtual void Init(Blackboard blackboard)
    {
        this.blackboard = blackboard;
        SetHeader();
    }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnRemove() { }
}
