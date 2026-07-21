using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControlAbility : BaseAbility
{
    [Header("高度偏移")]
    public float h;
    [Header("Z轴偏移距离")]
    public float length = 5f;
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
    private Vector3 _smoothedCameraPos;
    private Vector3 _smoothCameraPosVelocity;
    private Quaternion _smoothCameraRotation;
    private Quaternion _smoothCameraRotVelocity;
    private Transform _camera;
    private float _pitchDelta;
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
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

        blackboard.SetValue<float>("Pitch", SetPitch(_pitchDelta * pitchSpeed * Time.deltaTime));

        float yaw = rot.eulerAngles.y;
        // 合成相机的目标旋转：俯仰角 + 跟随的偏航角
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);

        // 计算相机位置：角色位置 + 旋转后的偏移向量
        // 偏移：基础高度向上 + 沿角色后方向后退length
        Vector3 offset = new Vector3(0, h, -length);
        Vector3 targetPos = pos + targetRot * offset;

        Vector3 fowardPos = pos + Vector3.up * h;
        Vector3 dir = targetPos - fowardPos;
        float armLength = dir.magnitude;
        if (Physics.Raycast(new Ray(fowardPos, dir.normalized), out var hit, armLength, layerMask))
        {
            armLength = hit.distance;
        }

        targetPos = fowardPos + dir.normalized * armLength;
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
}
