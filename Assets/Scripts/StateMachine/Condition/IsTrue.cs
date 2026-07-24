using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIsTrueCondition", menuName = "StateMachine/Condition/IsTrue")]
public class IsTrue : BaseCondition
{
    public string lockStateArgName = "IsLockState";
    public override bool IsCompleted(Blackboard blackboard)
    {
        if (!blackboard.GetValue<bool>(lockStateArgName, out var isTrue))
            Debug.LogError($"【状态机转换条件】{lockStateArgName}参数获取失败");
        return isTrue;
    }
}
