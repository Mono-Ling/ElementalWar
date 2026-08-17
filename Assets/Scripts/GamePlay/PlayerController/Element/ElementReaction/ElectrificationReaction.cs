using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 感电反应
/// 水电
/// 添加感电buff
/// </summary>
public class ElectrificationReaction : BaseElementReaction
{
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        elementBuffSet?.AddElementBuff<ElectrificationBuff>();
        return true;
    }
    public override int GetDamage(ElementType attackElement, int damage)
    => 0;
}
