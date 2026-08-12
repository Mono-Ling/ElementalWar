using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冻结反应
/// 水冰
/// 添加冻结buff
/// </summary>
public class FrozenReaction : BaseElementReaction
{
    public float frozenTime = 3f;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);

        // 添加冻结buff
        elementBuffSet?.AddElementBuff<FrozenBuff>(() => new(frozenTime));
        return true;
    }
}
