using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 融化反应
/// 冰火
/// 伤害倍率、移除冻结buff
/// </summary>
public class MeltReaction : BaseElementReaction
{
    [Header("攻击元素为火")]
    public float fireToIceNum = 2f;
    [Header("攻击元素为冰")]
    public float iceToFireNum = 1.5f;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);

        elementBuffSet?.TryRemoveElementBuff<FrozenBuff>();
        return true;
    }
    public override int GetDamage(ElementType attackElement, int damage)
    => attackElement switch
    {
        ElementType.Fire => Mathf.CeilToInt(damage * fireToIceNum),
        ElementType.Ice => Mathf.CeilToInt(damage * iceToFireNum),
        _ => 0
    };
}
