using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlAbility : BaseAbility
{
    [Header("相机跟随点偏移（相对角色位置）")]
    public Vector3 followOffset = new Vector3(0, 1, 0);
    [Header("相机目标点偏移（相对角色位置）")]
    public Vector3 targetOffset = new Vector3(0, 1.5f, -2);
    [Header("俯仰度")]
    public float pitch = 15;
    [Header("俯仰区间")]
    public float maxPitch = 50f;
    public float minPitch = -70f;
    [Header("相机参与碰撞层级")]
    public LayerMask layerMask;
    [Header("缓动时间")]
    public float posSmoothTime = 0.05f;
    public float rotSmoothTime = 0.1f;
    [Header("俯仰角改变速度")]
    public float pitchSpeed = 3;
    [Header("射击抖动范围")]
    public float maxRadius = 0.5f;
    [Header("射击俯仰角增加速度")]
    public float pitchAddSpeed = 1f;
    private Vector3 _smoothedCameraPos;
    private Vector3 _smoothCameraPosVelocity;
    private Quaternion _smoothCameraRotation;
    private Quaternion _smoothCameraRotVelocity;
    private Transform _camera;
    private float _pitchDelta;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        _camera = Camera.main.transform;
        AddInputPerformedListener("Rotation", OnRotationPerformed);
        AddInputCanceledListener("Rotation", OnRotationCanceled);

        _smoothedCameraPos = _camera.position;
        _smoothCameraRotation = _camera.rotation;
    }
    private void OnRotationPerformed(InputAction.CallbackContext context)
    => _pitchDelta = context.ReadValue<Vector2>().y;
    private void OnRotationCanceled(InputAction.CallbackContext context)
    => _pitchDelta = 0;
    public override void OnFixedUpdate()
    {
        blackboard.GetValue<Quaternion>("Rotation", out var rot);
        blackboard.GetValue<Vector3>("Position", out var pos);

        float yaw = rot.eulerAngles.y;
        // 合成相机的目标旋转：俯仰角 + 跟随的偏航角
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPos = pos + targetRot * targetOffset;

        Vector3 fowardPos = abilitySystem.transform.TransformPoint(followOffset);
        Vector3 dir = targetPos - fowardPos;
        float armLength = dir.magnitude;
        if (Physics.Raycast(new Ray(fowardPos, dir.normalized), out var hit, armLength, layerMask))
        {
            armLength = hit.distance;
        }

        targetPos = fowardPos + dir.normalized * armLength;

        blackboard.GetValue<float>("FireProgress", out var shootOffsetPower);
        targetPos = OnShoot(targetPos, shootOffsetPower);

        var pitchShootOffset = pitchAddSpeed * shootOffsetPower * Time.deltaTime;
        var pitchDelta = _pitchDelta * pitchSpeed * Time.deltaTime + pitchShootOffset;
        blackboard.SetValue<float>("Pitch", SetPitch(pitchDelta));

        _smoothedCameraPos = Vector3.SmoothDamp(_smoothedCameraPos, targetPos, ref _smoothCameraPosVelocity, posSmoothTime);
        _camera.position = _smoothedCameraPos;

        _smoothCameraRotation = Math.SmoothDamp(_smoothCameraRotation, targetRot, ref _smoothCameraRotVelocity, rotSmoothTime);
        _camera.rotation = _smoothCameraRotation;
    }
    private float SetPitch(float delta)
    {
        pitch += delta;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        float pitchNum = -pitch;
        if (pitchNum < 0)
            pitchNum /= Mathf.Max(-minPitch, 0.01f);
        else
            pitchNum /= Mathf.Max(maxPitch, 0.01f);
        return pitchNum;
    }
    private Vector3 OnShoot(Vector3 currPos, float progress)
    {
        if (progress == 0)
            return currPos;
        var angle = Random.Range(0, Mathf.PI * 2);
        var radius = Random.Range(0, maxRadius);
        Vector2 offset = new(Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius);
        var length = offset.magnitude;

        length = Mathf.Lerp(0, length, progress);
        offset = offset.normalized * length;
        return currPos + new Vector3(offset.x, offset.y, 0);
    }
}
