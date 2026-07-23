using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ShootStateSynSend : BaseSynSend
{
    public bool isDebug;
    private BlackboardArg<bool> _shootArg;
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        blackboard.GetBlackboardArg<bool>("IsShoot", out var arg);
        _shootArg = arg;
        _shootArg.OnValueChange += OnIsShootChange;
        SetHeader(true);
    }
    private void OnIsShootChange(bool isShoot)
    {
        if (isDebug)
            Debug.Log($"【射击状态】：{isShoot}");
        ShootStateMessage message = new() { IsShoot = isShoot };
        Send(message);
    }
    public override void OnRemove()
    => _shootArg.OnValueChange -= OnIsShootChange;
}
