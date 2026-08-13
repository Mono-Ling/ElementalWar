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
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);

        elementBuffSet?.TryRemoveElementBuff<FrozenBuff>();
        return true;
    }
}
