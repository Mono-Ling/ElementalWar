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
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (elementBuffSet?.Contains<IntensificationBuff>() ?? true)
            return false;
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
        elementBuffSet?.AddElementBuff<IntensificationBuff>();
        return true;
    }
}
