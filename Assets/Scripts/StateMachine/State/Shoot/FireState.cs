using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFireState", menuName = "StateMachine/State/ShootState/FireState")]
public class FireState : State
{
    public string isCoolingArgName = "IsShootCooling";
    public string fireProgressArgName = "FireProgress";
    public string shootAdditiveLayerName = "Shoot Additive";
    public float delayTime = 0.2f;
    private float _enterTime;
    private Animator _animator;
    public override void OnEnter(Blackboard blackboard)
    {
        blackboard.SetValue(isCoolingArgName, false);
        _enterTime = Time.time;
        blackboard.GetValue("Animator", out _animator);
        _animator.SetLayerWeight(_animator.GetLayerIndex(shootAdditiveLayerName), 1);
    }
    public override void OnUpdate(Blackboard blackboard)
    {
        var diff = Time.time - _enterTime;
        if (diff >= delayTime)
        {
            blackboard.SetValue(isCoolingArgName, true);
            return;
        }
        var totalTime = Mathf.Max(delayTime, 0.01f);
        var progress = diff / totalTime;
        blackboard.SetValue(fireProgressArgName, progress);
    }
    public override void OnExit(Blackboard blackboard)
    {
        blackboard.SetValue(fireProgressArgName, 0f);
        _animator.SetLayerWeight(_animator.GetLayerIndex(shootAdditiveLayerName), 0);
    }
}