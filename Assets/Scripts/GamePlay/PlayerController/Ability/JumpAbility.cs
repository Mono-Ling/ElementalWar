using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpAbility : BaseAbility
{
    public float jumpPower = 1;
    public float groundCheckDistance = 0.2f;
    public float verticalVelocityThreshold = 0.2f;
    public LayerMask layerMask;
    private Rigidbody _rigidbody;
    private bool _isGrounded = true;
    private bool _isJump;
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
        _rigidbody = mainPlayer.gameObject.GetComponent<Rigidbody>();
        AddInputStartedListener("Jump", OnJumpInput);
        blackboard.SetValue("IsJump", false);
    }
    private void OnJumpInput(InputAction.CallbackContext context)
    {
        if (!_isGrounded)
            return;
        _rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
        _isJump = true;
    }
    public override void OnUpdate()
    {
        if (Physics.Raycast(new Ray(_rigidbody.position, Vector3.down),
            out var hit, 100, layerMask))
        {
            _isGrounded = hit.distance < groundCheckDistance;
            //blackboard.SetValue("DisToGround", hit.distance);
            blackboard.SetValue("IsGrounded", _isGrounded);
        }
        _isJump = _rigidbody.velocity.y > verticalVelocityThreshold;
        blackboard.SetValue("IsJump", _isJump);
    }
    public override void OnRemove()
    {
        RemoveInputStartedListener("Jump", OnJumpInput);
    }
}
