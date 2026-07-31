using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowAbility : BaseAbility
{
    private bool _canFire = true;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener("Throw", OnThrowStarted);
    }
    private void OnThrowStarted(InputAction.CallbackContext context)
    {
        blackboard.GetValue<bool>("IsThrowFire", out var isFire);
        if (isFire)
            return;
        blackboard.SetValue<bool>("IsThrow", true);

        _canFire = true;

        AddInputCanceledListener("Throw", OnThrowCanceled);
        AddInputStartedListener("ThrowCancel", OnThrowCancelStart);
    }
    private void OnThrowCanceled(InputAction.CallbackContext context)
    {
        blackboard.SetValue<bool>("IsThrow", false);
        if (!_canFire)
            return;
        blackboard.SetValue<bool>("IsThrowFire", true);
        Debug.Log("Throw");

        RemoveInputCanceledListener("Throw", OnThrowCanceled);
        RemoveInputStartedListener("ThrowCancel", OnThrowCancelStart);
    }

    private void OnThrowCancelStart(InputAction.CallbackContext context)
    {
        blackboard.SetValue<bool>("IsThrow", false);
        _canFire = false;

        RemoveInputCanceledListener("Throw", OnThrowCanceled);
        RemoveInputStartedListener("ThrowCancel", OnThrowCancelStart);
    }
    public override void OnRemove()
    => RemoveInputStartedListener("Throw", OnThrowStarted);
}
