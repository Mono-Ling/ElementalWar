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
    /// <returns>是否能够反应</returns>
    public virtual bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        GetContentDelta(ref beforeContent, ref afterContent);
        return true;
    }
    /// <summary>
    /// 按比例消耗
    /// </summary>
    /// <param name="contentA"></param>
    /// <param name="contentB"></param>
    /// <param name="num">消耗比例a/b</param>
    /// <returns></returns>
    protected float GetContentDelta(ref float contentA, ref float contentB,
    float num = 1)
    {
        num = Mathf.Clamp01(num);
        float deltaB;
        if (num <= 0f)
        {
            // num = 0 时 A 不参与消耗，B 全额消耗
            deltaB = contentB;
            contentB = 0f;
            return deltaB;
        }

        // 按 num:1 比例推导最大反应次数 k
        float k = Mathf.Min(contentA / num, contentB);
        float deltaA = k * num;
        deltaB = k;

        contentA -= deltaA;
        contentB -= deltaB;
        return deltaB;
    }
    protected void SendTo(IMessage message, bool isResponse = true)
    {
        if (message == null)
            return;
        UdpHeader udpHeader = new() { IsResponse = isResponse };
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(udpHeader, message));
    }
}