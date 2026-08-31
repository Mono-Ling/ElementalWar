using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class MoveAbility : BaseAbility, IEquatable<MoveAbility>
{
    public float moveSpeed;
    private Rigidbody _rigidbody;
    private Vector2 _moveInput;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        _rigidbody = abilitySystem.GetComponent<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = abilitySystem.gameObject.AddComponent<Rigidbody>();

        // 状态切换时能力重建，主动读取当前输入
        var moveAction = playerInput.actions["Move"];
        if (moveAction.enabled)
            _moveInput = moveAction.ReadValue<Vector2>();

        AddInputPerformedListener("Move", OnMovePerformed);
        AddInputCanceledListener("Move", OnMoveCanceled);
    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    => _moveInput = context.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext context)
    => _moveInput = Vector2.zero;

    public override void OnFixedUpdate()
    {
        // 只修改水平速度，保留 Y 轴由物理系统控制（重力、跳跃等）
        var horizontalVelocity = new Vector3(_moveInput.x, 0f, _moveInput.y);
        horizontalVelocity = _rigidbody.rotation * horizontalVelocity;
        if (horizontalVelocity != Vector3.zero)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
        }
        _rigidbody.velocity = new Vector3(horizontalVelocity.x, _rigidbody.velocity.y, horizontalVelocity.z);
        blackboard.SetValue<Vector3>("Position", _rigidbody.position);
    }
    public override void OnRemove()
    {
        RemoveInputPerformedListener("Move", OnMovePerformed);
        RemoveInputCanceledListener("Move", OnMoveCanceled);
    }

    public bool Equals(MoveAbility other)
    => moveSpeed == other.moveSpeed;
    public override bool Equals(object obj)
    {
        if (obj is not MoveAbility move)
            return false;
        return moveSpeed == move.moveSpeed;
    }
    public override int GetHashCode()
    => moveSpeed.GetHashCode();
}
