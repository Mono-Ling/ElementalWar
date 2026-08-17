using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootAbility : BaseAbility
{
    private static ElementType _elementType = ElementType.Fire;
    public static void SetShootElementType(ElementType element)
    => _elementType = element;
    [Range(0, ElementUtility.Content.STRONG)]
    public float attackElementContent = ElementUtility.Content.WEAK;
    public int attackDamage = 20;
    public float delayTime = 0.2f;// s
    private bool _isShoot;
    private float _preShootTime;
    private Camera _mainCamera;
    private ShootRequestMessage _shootReqMes = new() { Origin = new(), Dir = new(), ElementAttack = new() };
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
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

        _shootReqMes.Origin.Switch(origin);
        _shootReqMes.Dir.Switch(dir);
        _shootReqMes.ElementAttack.ElementType = ElementUtility.ToNumber(_elementType);
        _shootReqMes.ElementAttack.Content = attackElementContent;
        _shootReqMes.ElementAttack.Damage = attackDamage;

        UdpHeader udpHeader = new() { IsResponse = true };
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(udpHeader, _shootReqMes));
    }
}
