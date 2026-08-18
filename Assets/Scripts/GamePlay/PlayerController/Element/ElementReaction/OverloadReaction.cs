using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

/// <summary>
/// 超载反应
/// 雷火
/// 火元素爆炸
/// </summary>
public class OverloadReaction : BaseElementReaction
{
    public float radius = 5f;
    [Range(0, ElementUtility.Content.STRONG)]
    public float content = ElementUtility.Content.WEAK;
    public float damageNum = 3f;
    private ElementAttackMessage _attackMessage = new()
    {
        ElementType = ElementUtility.ToNumber(ElementType.Fire)
    };
    private AreaElementDamageMes _damageMes = new()
    {
        Center = new(),
        AoeType = AreaElementDamageMes.Types.AOEType.Explosion
    };

    public override int GetDamage(ElementType attackElement, int damage)
    {
        if (damage == 0)
            return damage;
        _attackMessage.Content = content;
        _attackMessage.Damage = Mathf.CeilToInt(damageNum * damage);
        SetAreaDamage(_damageMes, radius, _attackMessage);
        SendTo(_damageMes);
        return 0;
    }
}
