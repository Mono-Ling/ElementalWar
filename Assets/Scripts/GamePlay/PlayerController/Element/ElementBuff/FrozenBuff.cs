using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 冻结buff
/// </summary>
public class FrozenBuff : BaseElementBuff
{
    private float _frozenTime;
    private float _startTime;
    public FrozenBuff(float frozenTime)
    => _frozenTime = Mathf.Abs(frozenTime);
    public override void OnEnter()
    {
        _startTime = Time.time;
        blackboard.SetValue("IsFrozen", true);
        Debug.Log("【冻结Buff】开始冻结");
    }
    public override void OnExit()
    {
        blackboard.SetValue("IsFrozen", false);
        Debug.Log("【冻结Buff】结束冻结");
    }
    public override void OnConflict()
    => _startTime = Time.time;
    public override bool TryExit()
    => Time.time - _startTime >= _frozenTime;
}
