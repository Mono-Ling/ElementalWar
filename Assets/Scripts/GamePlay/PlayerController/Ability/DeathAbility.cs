using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeathAbility : BaseAbility
{
    public List<string> resetBoolArgs = new() { "IsShoot", "IsThrow", "IsJump", "IsReload", "IsAim", "IsFrozen" };
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        foreach (var arg in resetBoolArgs)
            blackboard.SetValue(arg, false);
    }
}
