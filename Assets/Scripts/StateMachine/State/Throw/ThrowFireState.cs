using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowFireState", menuName = "StateMachine/State/ThrowState/ThrowFireState")]
public class ThrowFireState : State
{
    public float delay = 1.5f;
    private int _fullpath = Animator.StringToHash("UpperBody_Additive.Throw.Throw_Far");
    public override void OnEnter(Blackboard blackboard)
    {
        blackboard.GetValue<Animator>("Animator", out var animator);
        animator.SetTrigger("Throw");
        if (blackboard.GetValue<PlayerGunController>("PlayerGunController", out var playerGun))
            playerGun.enabled = false;
        PublicMono.Instance.StartCoroutine(Delay(() => blackboard.SetValue("IsThrowFire", false)));
    }
    private IEnumerator Delay(Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }
    public override void OnExit(Blackboard blackboard)
    {
        if (blackboard.GetValue<PlayerGunController>("PlayerGunController", out var playerGun))
            playerGun.enabled = true;
    }
}
