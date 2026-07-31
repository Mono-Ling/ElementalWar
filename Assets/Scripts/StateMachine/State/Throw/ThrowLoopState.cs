using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowLoopState", menuName = "StateMachine/State/ThrowState/ThrowLoopState")]
public class ThrowLoopState : State
{
    public override void OnEnter(Blackboard blackboard)
    {
        blackboard.GetValue<Animator>("Animator", out var animator);
        animator?.SetBool("IsThrow", true);
        if (blackboard.GetValue<PlayerGunController>("PlayerGunController", out var playerGun))
            playerGun.enabled = false;
    }
    public override void OnExit(Blackboard blackboard)
    {
        blackboard.GetValue<Animator>("Animator", out var animator);
        animator?.SetBool("IsThrow", false);
        if (blackboard.GetValue<PlayerGunController>("PlayerGunController", out var playerGun))
            playerGun.enabled = true;
    }
}
