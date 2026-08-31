using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RunAbility : BaseAbility
{
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener("Run", OnRunStarted);
        AddInputCanceledListener("Run", OnRunCanceled);

        if (playerInput.actions["Run"].IsPressed())
            blackboard.SetValue("IsRun", true);
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener("Run", OnRunStarted);
        RemoveInputCanceledListener("Run", OnRunCanceled);

        blackboard.SetValue("IsRun", false);
    }
    private void OnRunStarted(InputAction.CallbackContext context)
    => blackboard.SetValue("IsRun", true);
    private void OnRunCanceled(InputAction.CallbackContext context)
    => blackboard.SetValue("IsRun", false);
    public override bool Equals(object obj)
    => obj is RunAbility;
    public override int GetHashCode()
    => typeof(RunAbility).GetHashCode();
}
