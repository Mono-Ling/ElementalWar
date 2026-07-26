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
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.1f;
    public float velocitySmoothTime = 0.1f;
    public LayerMask layerMask;

    private Blackboard _blackboard;
    private Rigidbody _rigidbody;

    //位置缓动相关
    private Vector3 _smoothedPos;
    private Vector3 _posSmoothRef;
    //旋转缓动相关
    private Quaternion _smoothedRot;
    private Quaternion _rotSmoothRef;
    //速度缓动相关
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

        targetPos = GetCurrPos(targetPos);
        targetRot = GetCurrRot(targetRot);
        // SmoothDamp 平滑
        Vector3 instantVelocity = _posSmoothRef;
        _smoothedVelocity = Vector3.SmoothDamp(
            _smoothedVelocity, instantVelocity,
            ref _velocitySmoothRef, velocitySmoothTime);

        _rigidbody.MovePosition(targetPos);
        _rigidbody.MoveRotation(targetRot.normalized);

        _blackboard.GetValue<Animator>(animatorName, out var animator);
        Vector3 localVelocity = transform.InverseTransformDirection(_smoothedVelocity);
        animator?.SetFloat("MoveX", localVelocity.x);
        animator?.SetFloat("MoveY", localVelocity.z);
        animator?.SetFloat("MoveSpeed", _smoothedVelocity.magnitude);

        _blackboard.GetValue<float>(pitchArgName, out var pitch);
        animator?.SetFloat("AimY", pitch);

        SetJumpAnimation(animator);
    }
    private Vector3 GetCurrPos(Vector3 targetPos)
    {
        _smoothedPos = Vector3.SmoothDamp(_smoothedPos,
        targetPos, ref _posSmoothRef, positionSmoothTime);
        return _smoothedPos;
    }
    private Quaternion GetCurrRot(Quaternion targetRot)
    {
        _smoothedRot = Tools.Math.SmoothDamp(_smoothedRot,
        targetRot, ref _rotSmoothRef, rotationSmoothTime, deltaTime: Time.fixedDeltaTime);
        return _smoothedRot;
    }
    private void SetJumpAnimation(Animator animator)
    {
        _blackboard.GetValue<bool>("IsJump", out var isJump);
        _blackboard.GetValue<bool>("IsGrounded", out var isGrounded);
        // _blackboard.GetValue<float>("DisToGround", out var disToGround);
        animator?.SetBool("IsJump", isJump);
        animator?.SetBool("IsGrounded", isGrounded);
        animator?.SetFloat("VerticalVelocity", _rigidbody.velocity.y);

        if (Physics.Raycast(new Ray(_rigidbody.position, Vector3.down),
            out var hit, 100, layerMask))
        {
            animator?.SetFloat("DisToGround", hit.distance);
        }
    }
}
