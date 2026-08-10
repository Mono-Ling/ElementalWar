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
    private ElementBuffSet _elementBuffSet;
    public override void OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);

        if (blackboard.GetValue("ElementBuffSet", out _elementBuffSet))
            _elementBuffSet?.TryRemoveElementBuff<FrozenBuff>(() => new(default));
        else
            Debug.Log("【融化反应】元素Buff集合获取失败");
    }
}
