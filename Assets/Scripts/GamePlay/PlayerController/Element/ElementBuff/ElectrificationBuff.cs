using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感电buff
/// </summary>
public class ElectrificationBuff : BaseElementBuff
{
    private float _delay;
    private float _preTime;
    public ElectrificationBuff(float delay) => _delay = Mathf.Abs(delay);
    /// <summary>
    /// 水或雷元素附着消失退出
    /// </summary>
    /// <returns>是否退出</returns>
    public override bool TryExit()
    => !elementReceiver.TotalElement.HasFlag(ElementType.Water)
    || !elementReceiver.TotalElement.HasFlag(ElementType.Thunder);
    public override void OnEnter() => Debug.Log("【感电Buff】进入感电buff");
    public override void OnExit() => Debug.Log("【感电Buff】退出感电buff");
    public override void OnUpdate()
    {
        if (Time.time - _preTime < _delay)
            return;
        OnElectrification();
        _preTime = Time.time;
    }
    private void OnElectrification()
    {
        Debug.Log("【感电Buff】感电伤害");
    }
}
