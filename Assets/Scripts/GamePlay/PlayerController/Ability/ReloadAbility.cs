using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReloadAbility : BaseAbility
{
    public float reloadTime = 3f;
    private Coroutine _coroutine;
    private float _progress;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener("Reload", OnReloadStarted);
        AddInputStartedListener("Fire", OnFireStarted);
    }
    private void OnReloadStarted(InputAction.CallbackContext context)
    {
        if (_coroutine != null)
            return;
        blackboard.SetValue("IsReload", true);
        _coroutine = abilitySystem.StartCoroutine(ReloadUpdate());
    }
    private void OnFireStarted(InputAction.CallbackContext context)
    {
        if (_coroutine == null)
            return;
        abilitySystem.StopCoroutine(_coroutine);

        Debug.Log("换弹打断");

        OnReloadEnd();
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener("Reload", OnReloadStarted);
        RemoveInputStartedListener("Fire", OnFireStarted);
    }
    private IEnumerator ReloadUpdate()
    {
        blackboard.SetValue("ReloadProgress", _progress);
        float startTime = Time.time;
        while (Time.time - startTime < reloadTime)
        {
            _progress = (Time.time - startTime) / reloadTime;
            _progress = Mathf.Clamp01(_progress);
            blackboard.SetValue("ReloadProgress", _progress);
            yield return null;
        }
        OnReloadEnd();
        OnReloadOver();
    }
    private void OnReloadOver()
    {
        Debug.Log("换弹结束");
        if (blackboard.GetValue<ElementType>("AttackElementType", out var type))
            ShootAbility.SetShootElementType(type);
    }
    private void OnReloadEnd()
    {
        _progress = 0;
        blackboard.SetValue("ReloadProgress", _progress);
        blackboard.SetValue("IsReload", false);
        _coroutine = null;
    }
    public override bool Equals(object obj)
    => obj.GetType() == this.GetType();
    public override int GetHashCode()
    => GetType().GetHashCode();
}
