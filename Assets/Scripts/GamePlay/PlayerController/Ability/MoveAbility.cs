using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class MoveAbility : BaseAbility
{
    public float moveSpeed;
    private Rigidbody _rigidbody;
    private Vector2 _moveInput;
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
        _rigidbody = mainPlayer.GetComponent<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = mainPlayer.gameObject.AddComponent<Rigidbody>();

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
}
