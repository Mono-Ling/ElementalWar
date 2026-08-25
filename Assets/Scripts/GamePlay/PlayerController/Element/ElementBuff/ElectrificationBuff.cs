using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感电buff
/// </summary>
public class ElectrificationBuff : BaseElementBuff
{
    [Range(0, ElementUtility.Content.STRONG)]
    public float content = ElementUtility.Content.WEAK;
    public int damage = 50;
    [Header("感电UI显示")]
    public string electrificationName = "感电";
    public Color electrificationColor;
    public float delay;
    private float _preTime;
    private int _attackPlayerId;
    /// <summary>
    /// 水或雷元素附着消失退出
    /// </summary>
    /// <returns>是否退出</returns>
    public override bool TryExit()
    => !elementAttachment.TotalElement.HasFlag(ElementType.Water)
    || !elementAttachment.TotalElement.HasFlag(ElementType.Thunder);
    public override void OnEnter()
    {
        Debug.Log("【感电Buff】进入感电buff");
        _preTime = Time.time - delay;
        _attackPlayerId = MainPlayerHP.AttackFromPlayerId;
    }
    public override void OnConflict()
    => _attackPlayerId = MainPlayerHP.AttackFromPlayerId;
    public override void OnExit() => Debug.Log("【感电Buff】退出感电buff");
    public override void OnUpdate()
    {
        if (Time.time - _preTime < delay)
            return;
        OnElectrification();
        _preTime = Time.time;
    }
    private void OnElectrification()
    {
        Debug.Log("【感电Buff】感电伤害");
        var finalDamage = damage;
        var finalContent = content;
        TriggerAttackListener(ElementType.Thunder, ref finalContent, ref finalDamage);
        if (finalDamage == 0 || finalContent < 0.001)
            return;
        playerHP?.SetAttackFrom(_attackPlayerId);
        playerHP?.ReduceHP(finalDamage, electrificationColor);
        playerHP?.SetAttackFrom();

        dynamicTextCreator?.ShowTextUI(electrificationName, electrificationColor);
    }
}
