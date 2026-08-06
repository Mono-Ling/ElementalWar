using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFOVAbility : BaseAbility, IEquatable<CameraFOVAbility>
{
    public float FOV = 60;
    public float smoothTime = 0.1f;
    private float _smoothRef;
    private float _smoothedFOV;
    private Camera _camera;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        _camera = Camera.main;
        if (_camera == null)
        {
            Debug.LogError("【相机FOV控制Ability】相机获取失败");
            return;
        }
        _smoothedFOV = _camera.fieldOfView;
    }
    public override void OnLateUpdate()
    {
        _smoothedFOV = Mathf.SmoothDamp(_smoothedFOV, FOV, ref _smoothRef, smoothTime);
        _camera.fieldOfView = _smoothedFOV;
    }
    public override bool Equals(object obj)
    {
        if (obj is not CameraFOVAbility fovAbility)
            return false;
        return fovAbility.FOV == FOV;
    }
    public override int GetHashCode() => FOV.GetHashCode();
    public bool Equals(CameraFOVAbility other)
    => FOV == other.FOV;
}
