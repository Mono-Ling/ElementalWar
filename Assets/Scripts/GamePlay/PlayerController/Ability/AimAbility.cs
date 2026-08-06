using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimAbility : BaseAbility
{
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener("Aim", OnAimStarted);
        AddInputCanceledListener("Aim", OnAimCanceled);
    }
    private void OnAimStarted(InputAction.CallbackContext context)
    => blackboard.SetValue<bool>("IsAim", true);
    private void OnAimCanceled(InputAction.CallbackContext context)
    => blackboard.SetValue<bool>("IsAim", false);
    public override void OnRemove()
    {
        RemoveInputStartedListener("Aim", OnAimStarted);
        RemoveInputCanceledListener("Aim", OnAimCanceled);
    }
    public override bool Equals(object obj)
    => GetType() == obj.GetType();
    public override int GetHashCode()
    => GetType().GetHashCode();
}
