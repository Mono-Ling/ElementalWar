using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsLockState : BaseCondition
{
    public string lockStateArgName = "IsLockState";
    private BlackboardArg<bool> _isLockArg;
    public override bool IsCompleted(Blackboard blackboard)
    {
        if (_isLockArg == null)
            if (blackboard.GetBlackboardArg<bool>(lockStateArgName, out var arg))
                _isLockArg = arg;
            else
                Debug.LogError($"【状态机转换条件】{lockStateArgName}参数获取失败");
        return _isLockArg != null ? _isLockArg.value : false;
    }
}
