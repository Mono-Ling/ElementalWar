using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIsNetConnected", menuName = "StateMachine/Condition/IsNetConnected")]
public class IsNetConnected : BaseCondition
{
    public override bool IsCompleted(Blackboard blackboard)
    => NetManager.Instance.IsStart;
}
