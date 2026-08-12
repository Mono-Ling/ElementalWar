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
    public float delay = 1f;
    private ElementBuffSet _elementBuffSet;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (blackboard.GetValue("ElementBuffSet", out _elementBuffSet))
            _elementBuffSet?.AddElementBuff<ElectrificationBuff>(() => new(delay));
        else
            Debug.LogWarning("【感电反应】元素Buff集合获取失败");
        return true;
    }
}
