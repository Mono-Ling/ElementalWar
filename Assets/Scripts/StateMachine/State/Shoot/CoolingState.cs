using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmptyState", menuName = "StateMachine/State/ShootState/CoolingState")]
public class CoolingState : State
{
    public string isCoolingArgName = "IsShootCooling";
    public string fireProgressArgName = "FireProgress";
    public float delayTime = 0.2f;
    private float _enterTime;
    public override void OnEnter(Blackboard blackboard)
    {
        blackboard.SetValue(isCoolingArgName, true);
        blackboard.SetValue(fireProgressArgName, 1f);
        _enterTime = Time.time;
    }
    public override void OnUpdate(Blackboard blackboard)
    {
        var diff = Time.time - _enterTime;
        if (diff >= delayTime)
        {
            blackboard.SetValue(isCoolingArgName, false);
            blackboard.SetValue(fireProgressArgName, 0f);
            return;
        }
        var totalTime = Mathf.Max(delayTime, 0.01f);
        var progress = 1 - diff / totalTime;
        blackboard.SetValue(fireProgressArgName, progress);
    }
    public override void OnExit(Blackboard blackboard)
    {
        blackboard.SetValue(fireProgressArgName, 0f);
    }
}
