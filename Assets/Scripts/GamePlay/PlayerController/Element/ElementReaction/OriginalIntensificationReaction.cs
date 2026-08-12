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
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        IntensificationBuff buff = new(duration, delayListen);
        if (elementBuffSet?.Contains(buff) ?? true)
            return false;
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
        elementBuffSet?.AddElementBuff(buff);
        return true;
    }
}
