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
    public float delay = 0.25f;
    public float speed = 0.125f;
    [Header("先手元素消耗量/后手元素消耗量")]
    public float num = 0.5f;
    private ElementBuffSet _elementBuffSet;
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        if (_elementBuffSet == null)
        {
            if (!blackboard.GetValue("ElementBuffSet", out _elementBuffSet))
                Debug.LogWarning("【燃烧反应】元素Buff集合获取失败");
        }
        BurnBuff buff = new(delay, speed);
        if (_elementBuffSet?.Contains(buff) ?? true)
            return false;

        GetContentDelta(ref beforeContent, ref afterContent, num);
        _elementBuffSet.AddElementBuff(buff);
        return true;
    }
}
