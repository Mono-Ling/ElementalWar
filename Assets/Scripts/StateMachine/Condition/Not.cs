using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNotCondition", menuName = "StateMachine/Condition/Not")]
public class Not : BaseCondition
{
    public BaseCondition condition;
    public override bool IsCompleted(Blackboard blackboard)
    {
        return !condition.IsCompleted(blackboard);
    }
    void OnValidate()
    {
        if (condition == null)
            Debug.LogWarning("【状态机转换条件】参数为空");
    }
}
