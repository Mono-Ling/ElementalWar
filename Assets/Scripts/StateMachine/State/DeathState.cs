using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeathState", menuName = "StateMachine/State/DeathState")]
public class DeathState : State
{
    public string animatorArgName = "Animator";
    public string additiveLayerName = "UpperBody Additive";
    public override void OnEnter(Blackboard blackboard)
    {
        if (!blackboard.GetValue<Animator>(animatorArgName, out var animator))
            return;
        animator.SetBool("IsDeath", true);
        animator.SetLayerWeight(animator.GetLayerIndex(additiveLayerName), 0);
    }
    public override void OnExit(Blackboard blackboard)
    {
        if (!blackboard.GetValue<Animator>(animatorArgName, out var animator))
            return;
        animator.SetBool("IsDeath", false);
        animator.SetLayerWeight(animator.GetLayerIndex(additiveLayerName), 1);
    }
    public override bool Equals(object other)
    {
        if (other is not DeathState death)
            return false;
        return true;
    }
    public override int GetHashCode()
    => typeof(DeathState).GetHashCode();
}
