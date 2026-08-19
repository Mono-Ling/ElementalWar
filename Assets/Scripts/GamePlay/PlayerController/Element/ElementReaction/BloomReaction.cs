using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 绽放
/// 水草
/// 产生草种子
/// </summary>
public class BloomReaction : BaseElementReaction
{
    public float delay = 0.5f;
    public int maxCount = 2;
    private float _preTime;
    private int _currCount;
    public override void Init(ElementReceiver receiver, ElementBuffSet buffSet, Blackboard blackboard)
    {
        base.Init(receiver, buffSet, blackboard);
        _preTime = Time.time - delay;
    }
    public override bool OnReaction(
        ElementType beforeElement, ElementType afterElement,
        ref float beforeContent, ref float afterContent)
    {
        bool time = Time.time - _preTime < delay;
        bool count = _currCount >= maxCount;
        if (time && count)
            return false;
        else if (time && !count)
            _currCount++;
        else
        {
            _preTime = Time.time;
            _currCount = 1;
        }

        DynamicSceneItemMgr.Instance.CreateLocalDynamicSceneItem<Vector3>
        (Message.DynamicSceneItemType.GrassCore,
        elementReceiver?.transform.position ?? Vector3.zero);
        return base.OnReaction(beforeElement, afterElement, ref beforeContent, ref afterContent);
    }
}
