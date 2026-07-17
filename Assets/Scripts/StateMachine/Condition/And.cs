using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAndCondition", menuName = "StateMachine/Condition/And")]
public class And : BaseCondition
{
    public BaseCondition A;
    public BaseCondition B;
    public override bool IsCompleted(Blackboard blackboard)
    {
        return A.IsCompleted(blackboard) && B.IsCompleted(blackboard);
    }
    void OnValidate()
    {
        if (A == null || B == null)
            Debug.LogWarning("【状态机转换条件】参数为空");
    }
}
