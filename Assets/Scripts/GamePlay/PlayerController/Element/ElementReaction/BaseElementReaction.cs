using System;
using Google.Protobuf;
using Message;
using UnityEngine;

[Serializable]
public abstract class BaseElementReaction
{
    public string name;
    public Color color;
    protected Blackboard blackboard;
    protected ElementReceiver elementReceiver;
    public virtual void Init(ElementReceiver receiver, Blackboard blackboard)
    {
        elementReceiver = receiver;
        this.blackboard = blackboard;
    }
    /// <summary>
    /// 元素反应逻辑
    /// </summary>
    /// <param name="beforeElement">先手元素</param>
    /// <param name="afterElement">后手元素</param>
    /// <param name="beforeContent">先手元素量</param>
    /// <param name="afterContent">后手元素量</param>
    public virtual void OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    => GetContentDelta(ref beforeContent, ref afterContent);
    protected float GetContentDelta(ref float content1, ref float content2)
    {
        var delta = Mathf.Min(content1, content2);
        content1 -= delta;
        content2 -= delta;
        return delta;
    }
    protected void SendTo(IMessage message, bool isResponse = true)
    {
        if (message == null)
            return;
        UdpHeader udpHeader = new() { IsResponse = isResponse };
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(udpHeader, message));
    }
}