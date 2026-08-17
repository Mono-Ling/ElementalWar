using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蒸发反应
/// 水火
/// 伤害倍率
/// </summary>
public class EvaporationReaction : BaseElementReaction
{
    [Header("攻击元素为火")]
    public float fireToWhaterNum = 1.5f;
    [Header("攻击元素为水")]
    public float waterToFireNum = 2f;
    public override int GetDamage(ElementType attackElement, int damage)
    => attackElement switch
    {
        ElementType.Fire => Mathf.CeilToInt(damage * fireToWhaterNum),
        ElementType.Water => Mathf.CeilToInt(damage * waterToFireNum),
        _ => 0
    };
}
