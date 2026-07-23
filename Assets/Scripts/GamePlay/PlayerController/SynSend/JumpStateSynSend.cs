using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class JumpStateSynSend : BaseSynSend
{
    public bool isDebug;
    private BlackboardArg<bool> _isJumpArg;
    private BlackboardArg<bool> _isGroundedArg;
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        blackboard.GetBlackboardArg<bool>("IsJump", out var isJumpArg);
        blackboard.GetBlackboardArg<bool>("IsGrounded", out var isGoundedArg);
        _isJumpArg = isJumpArg;
        _isGroundedArg = isGoundedArg;
        _isJumpArg.OnValueChange += OnIsJumpChange;
        _isGroundedArg.OnValueChange += OnIsGroundedChange;
        SetHeader(true);
    }
    private void OnIsJumpChange(bool isJump)
    => OnJumpStateChange(isJump, _isGroundedArg.value);
    private void OnIsGroundedChange(bool isGrounded)
    => OnJumpStateChange(_isJumpArg.value, isGrounded);
    private void OnJumpStateChange(bool isJump, bool isGrounded)
    {
        if (isDebug)
            Debug.Log($"【跳跃参数修改】isJump:{isJump}isGrounded:{isGrounded}");
        JumpStateMessage message = new() { IsJump = isJump, IsGrounded = isGrounded };
        Send(message);
    }
    public override void OnRemove()
    {
        _isJumpArg.OnValueChange -= OnIsJumpChange;
        _isGroundedArg.OnValueChange -= OnIsGroundedChange;
    }
}
