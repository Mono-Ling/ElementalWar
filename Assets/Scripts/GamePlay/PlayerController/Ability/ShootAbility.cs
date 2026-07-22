using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootAbility : BaseAbility
{
    public float delayTime = 0.2f;// s
    private bool _isShoot;
    private float _preShootTime;
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
        AddInputStartedListener("Fire", OnFireStarted);
        AddInputCanceledListener("Fire", OnFireCanceled);
    }
    private void OnFireStarted(InputAction.CallbackContext context)
    {
        blackboard.SetValue<bool>("IsShoot", true);
        _isShoot = true;
        _preShootTime = 0;
    }
    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        blackboard.SetValue<bool>("IsShoot", false);
        _isShoot = false;
    }
    public override void OnUpdate()
    {
        if (!_isShoot || _preShootTime + delayTime > Time.time)
            return;
        _preShootTime = Time.time;
        Debug.Log("Fire");
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener("Fire", OnFireStarted);
        RemoveInputCanceledListener("Fire", OnFireCanceled);
    }
}
