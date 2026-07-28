using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootAbility : BaseAbility
{
    public float delayTime = 0.2f;// s
    private bool _isShoot;
    private float _preShootTime;
    private Camera _mainCamera;
    private Vector3Message _originMes = new();
    private Vector3Message _dirMes = new();
    private ShootRequestMessage _shootReqMes = new();
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
        AddInputStartedListener("Fire", OnFireStarted);
        AddInputCanceledListener("Fire", OnFireCanceled);
        _mainCamera = Camera.main;
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
        OnShoot();
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener("Fire", OnFireStarted);
        RemoveInputCanceledListener("Fire", OnFireCanceled);
    }
    private void OnShoot()
    {
        Vector3 screenOrigin = new(Screen.width / 2, Screen.height / 2, 0);
        Vector3 origin = _mainCamera.ScreenToWorldPoint(screenOrigin);
        Vector3 dir = _mainCamera.transform.forward;

        _originMes.Switch(origin);
        _dirMes.Switch(dir);

        _shootReqMes.Origin = _originMes;
        _shootReqMes.Dir = _dirMes;

        UdpHeader udpHeader = new() { IsResponse = true };
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(udpHeader, _shootReqMes));
    }
}
