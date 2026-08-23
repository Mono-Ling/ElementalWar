using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFrozenState", menuName = "StateMachine/State/FrozenState")]
public class FrozenState : State, IEquatable<FrozenState>
{
    public string animatorArgName = "Animator";
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard.GetValue<Animator>(animatorArgName, out var animator))
            animator.speed = 0;
    }
    public override void OnExit(Blackboard blackboard)
    {
        if (blackboard.GetValue<Animator>(animatorArgName, out var animator))
            animator.speed = 1;
    }
    public override bool Equals(object other)
    {
        if (other is not FrozenState frozen)
            return false;
        return Equals(frozen);
    }
    public override int GetHashCode()
    => typeof(FrozenState).GetHashCode();
    public bool Equals(FrozenState other)
    => other != null && other.GetType() == GetType();
}
