using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class DeathStateSynSend : BaseSynSend
{
    private BlackboardArg<bool> _deathArg;
    private DeathStateMessage _deathMes = new();
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        if (!blackboard.GetBlackboardArg("IsDeath", out _deathArg))
        {
            Debug.LogError("【死亡状态同步发送】冰冻黑板参数获取失败");
            return;
        }
        _deathArg.OnValueChange += OnDeathChange;
        SetHeader(true);
    }
    public override void OnRemove()
    {
        if (_deathArg != null)
            _deathArg.OnValueChange -= OnDeathChange;
    }
    private void OnDeathChange(bool isDeath)
    {
        _deathMes.IsDeath = isDeath;
        Send(_deathMes);
    }
}
