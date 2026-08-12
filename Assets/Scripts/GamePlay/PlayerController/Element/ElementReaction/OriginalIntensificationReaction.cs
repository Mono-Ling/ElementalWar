using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 原激化反应
/// 草雷
/// 添加激化buff
/// </summary>
public class OriginalIntensificationReaction : BaseElementReaction
{
    public float duration = 10f;
    public float delayListen = 0.1f;
    private ElementBuffSet _elementBuffSet;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (_elementBuffSet == null)
        {
            if (!blackboard.GetValue("ElementBuffSet", out _elementBuffSet))
                Debug.LogWarning("【原激化反应】元素Buff集合获取失败");
        }
        IntensificationBuff buff = new(duration, delayListen);
        if (_elementBuffSet?.Contains(buff) ?? true)
            return false;
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
        _elementBuffSet?.AddElementBuff(buff);
        return true;
    }
}
