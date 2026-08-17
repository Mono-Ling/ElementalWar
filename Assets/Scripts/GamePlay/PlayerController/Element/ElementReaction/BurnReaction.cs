using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 燃烧反应
/// 火草
/// 添加燃烧buff
/// </summary>
public class BurnReaction : BaseElementReaction
{
    [Header("先手元素消耗量/后手元素消耗量")]
    public float num = 0.5f;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (elementBuffSet?.Contains<BurnBuff>() ?? true)
            return false;
        GetContentDelta(ref beforeContent, ref afterContent, num);
        elementBuffSet?.AddElementBuff<BurnBuff>();
        return true;
    }
    public override int GetDamage(ElementType attackElement, int damage)
    => 0;
}
