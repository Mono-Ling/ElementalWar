using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowAbility : BaseAbility
{
    public Vector3 offset = new Vector3(0.5f, 1, 0);
    public float throwAngleSpeed = 3f;
    public float throwForce = 10f;
    public float defaultThrowAngle = 45f;
    public float maxThrowAngle = 80f;
    public float minThrowAngle = 10f;
    private bool _canFire = true;
    private bool _isThrowing = false;
    private ThrowTrack _throwTrack;
    private float _throwAngle;
    private float _throwAngleDelta;
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
        _isThrowing = true;

        var obj = MonoObjectPool.Instance.GetObject("ThrowTrack");
        obj.transform.position = abilitySystem.transform.TransformPoint(offset);
        _throwTrack = obj.GetComponent<ThrowTrack>();
        _throwAngle = defaultThrowAngle;

        AddInputPerformedListener("Rotation", OnRotationPerformed);
        AddInputCanceledListener("Rotation", OnRotationCanceled);
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

        OnThrowEnd();
    }

    private void OnThrowCancelStart(InputAction.CallbackContext context)
    {
        _canFire = false;
        OnThrowEnd();
    }
    private void OnThrowEnd()
    {
        MonoObjectPool.Instance.PutObject(_throwTrack.gameObject);

        blackboard.SetValue<bool>("IsThrow", false);
        _isThrowing = false;
        RemoveInputPerformedListener("Rotation", OnRotationPerformed);
        RemoveInputCanceledListener("Rotation", OnRotationCanceled);
        RemoveInputCanceledListener("Throw", OnThrowCanceled);
        RemoveInputStartedListener("ThrowCancel", OnThrowCancelStart);
    }
    private void OnRotationPerformed(InputAction.CallbackContext context)
    => _throwAngleDelta = context.ReadValue<Vector2>().y;
    private void OnRotationCanceled(InputAction.CallbackContext context)
    => _throwAngleDelta = 0;
    override public void OnLateUpdate()
    {
        if (!_isThrowing || _throwTrack == null)
            return;

        _throwAngle += _throwAngleDelta * Time.deltaTime * throwAngleSpeed;
        _throwAngle = Mathf.Clamp(_throwAngle, minThrowAngle, maxThrowAngle);
        Vector3 throwDir = Quaternion.Euler(-_throwAngle, 0, 0) * abilitySystem.transform.forward;
        Vector3 throwPos = abilitySystem.transform.TransformPoint(offset);

        _throwTrack.UpdateTrack(throwDir.normalized, throwForce, throwPos);
    }
    public override void OnRemove()
    => RemoveInputStartedListener("Throw", OnThrowStarted);
    public override bool Equals(object obj)
    {
        if (obj is not ThrowAbility other)
            return false;
        return this.GetType() == other.GetType();
    }
    public override int GetHashCode()
    => this.GetType().GetHashCode();
}
