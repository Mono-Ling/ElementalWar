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
        var pos = new Vector3(_moveInput.x, 0, _moveInput.y) * moveSpeed * Time.fixedDeltaTime;
        pos = _rigidbody.rotation * pos;
        _rigidbody.MovePosition(_rigidbody.position + pos);
        blackboard.SetValue<Vector3>("Position", _rigidbody.position);
    }
    public override void OnRemove()
    {
        RemoveInputPerformedListener("Move", OnMovePerformed);
        RemoveInputCanceledListener("Move", OnMoveCanceled);
    }
}
