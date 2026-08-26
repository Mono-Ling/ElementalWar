using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class FrozenStateSynSend : BaseSynSend
{
    private BlackboardArg<bool> _frozenArg;
    private FrozenStateMessage _frozenStateMes = new();
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        if (!blackboard.GetBlackboardArg("IsFrozen", out _frozenArg))
        {
            Debug.LogError("【冰冻状态同步发送】冰冻黑板参数获取失败");
            return;
        }
        _frozenArg.OnValueChange += OnFrozenChange;
        SetHeader(true);
        OnFrozenChange(_frozenArg.value);
    }
    public override void OnRemove()
    {
        if (_frozenArg != null)
            _frozenArg.OnValueChange -= OnFrozenChange;
    }
    private void OnFrozenChange(bool isFrozen)
    {
        _frozenStateMes.IsFrozen = isFrozen;
        Send(_frozenStateMes);
    }
}
