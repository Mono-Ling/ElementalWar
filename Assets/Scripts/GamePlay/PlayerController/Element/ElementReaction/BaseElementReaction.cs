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
    protected ElementBuffSet elementBuffSet;
    public virtual void Init(ElementReceiver receiver, ElementBuffSet buffSet, Blackboard blackboard)
    {
        elementReceiver = receiver;
        elementBuffSet = buffSet;
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
    /// 获取元素反应伤害
    /// </summary>
    /// <param name="damage">初始伤害值</param>
    /// <param name="beforeDelta">附着元素消耗量</param>
    /// <param name="afterDelta">攻击元素消耗量</param>
    /// <returns>元素反应伤害</returns>
    public virtual int GetDamage(int damage, float beforeDelta, float afterDelta)
    {
        return damage + Mathf.CeilToInt(damage * afterDelta);
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
    protected void SetAreaDamage(
        AreaElementDamageMes areaMes,
        float radius, params ElementAttackMessage[] attackMes)
    {
        if (areaMes == null || attackMes == null)
            return;
        areaMes.Center = areaMes.Center ?? new();
        areaMes.Center.Switch(elementReceiver?.transform.position ?? Vector3.zero);
        areaMes.Radius = radius;
        areaMes.ElementAttack.Clear();
        foreach (var attack in attackMes)
        {
            if (attack != null)
                areaMes.ElementAttack.Add(attack);
        }
    }
}