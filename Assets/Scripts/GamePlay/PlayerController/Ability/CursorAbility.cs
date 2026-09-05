using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorAbility : BaseAbility
{
    public string unlockInput = "UnlockCursor";
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener(unlockInput, OnUnlockCursorStarted);
        AddInputCanceledListener(unlockInput, OnUnlockCursorCeneled);
        Cursor.lockState = CursorLockMode.Locked;
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener(unlockInput, OnUnlockCursorStarted);
        RemoveInputCanceledListener(unlockInput, OnUnlockCursorCeneled);
        Cursor.lockState = CursorLockMode.None;
    }
    private void OnUnlockCursorStarted(InputAction.CallbackContext context)
    => Cursor.lockState = CursorLockMode.None;
    private void OnUnlockCursorCeneled(InputAction.CallbackContext context)
    => Cursor.lockState = CursorLockMode.Locked;
    public override bool Equals(object obj)
    => obj is CursorAbility;
    public override int GetHashCode()
    => typeof(CursorAbility).GetHashCode();
}
