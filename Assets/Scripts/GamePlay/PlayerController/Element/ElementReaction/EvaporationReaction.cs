using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蒸发反应
/// 水火
/// 伤害倍率
/// </summary>
public class EvaporationReaction : BaseElementReaction
{
    public override void OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
    }
}
