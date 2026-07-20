using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public string positionArgName = "Position";
    public string rotationArgName = "Rotation";
    public string animatorName = "Animator";
    public string pitchArgName = "Pitch";
    public float velocitySmoothTime = 0.1f;

    private Blackboard _blackboard;
    private Rigidbody _rigidbody;

    private Vector3 _smoothedVelocity;
    private Vector3 _velocitySmoothRef;

    private void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【PlayerMove】角色黑板获取失败");
            enabled = false;
            return;
        }
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            Debug.LogError("【PlayerMove】刚体获取失败");
            return;
        }
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
    }
    void FixedUpdate()
    {
        if (!_blackboard.GetValue<Vector3>(positionArgName, out var targetPos))
        {
            Debug.LogError("【Player】位置设置失败");
            return;
        }
        if (!_blackboard.GetValue<Quaternion>(rotationArgName, out var targetRot))
        {
            Debug.LogError("【Player】旋转设置失败");
            return;
        }
        // SmoothDamp 平滑
        Vector3 instantVelocity = _rigidbody.velocity;
        _smoothedVelocity = Vector3.SmoothDamp(
            _smoothedVelocity, instantVelocity,
            ref _velocitySmoothRef, velocitySmoothTime);

        _rigidbody.MovePosition(targetPos);
        _rigidbody.MoveRotation(targetRot);

        _blackboard.GetValue<Animator>(animatorName, out var animator);
        Vector3 localVelocity = transform.InverseTransformDirection(_smoothedVelocity);
        animator?.SetFloat("MoveX", localVelocity.x);
        animator?.SetFloat("MoveY", localVelocity.z);
        animator?.SetFloat("MoveSpeed", _smoothedVelocity.magnitude);

        _blackboard.GetValue<float>(pitchArgName, out var pitch);
        animator?.SetFloat("AimY", pitch);
    }
}
