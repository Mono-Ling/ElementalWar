using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

/// <summary>
/// 超导反应
/// 冰雷
/// 造成冰元素范围伤害
/// </summary>
public class SuperconductionReaction : BaseElementReaction
{
    public int damage = 30;
    [Range(0, ElementUtility.Content.STRONG)]
    public float areaDamageContent = ElementUtility.Content.WEAK;
    public float radius = 3f;
    private AreaElementDamageMes _areaAttack = new();
    private ElementAttackMessage _elementAttack = new();
    public override void Init(ElementReceiver receiver, ElementBuffSet buffSet, Blackboard blackboard)
    {
        base.Init(receiver, buffSet, blackboard);

        _elementAttack.ElementType = ElementUtility.ToNumber(ElementType.Ice);
        _elementAttack.Content = areaDamageContent;
        _elementAttack.Damage = damage;
    }
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        _elementAttack.FromPlayerId = MainPlayerHP.AttackFromPlayerId;
        SetAreaDamage(_areaAttack, radius, _elementAttack);
        SendTo(_areaAttack);

        return base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
    }
    public override int GetDamage(ElementType attackElement, int damage)
    => this.damage;
}
