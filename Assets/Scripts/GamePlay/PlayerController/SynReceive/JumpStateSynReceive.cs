using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class JumpStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Blackboard blackboard)
    {
        base.Init(otherPlayer, blackboard);
        AddListener<JumpStateMessage>(OnJumpStateSyn);
    }
    private void OnJumpStateSyn(JumpStateMessage message)
    {
        if (message == null)
            return;
        blackboard.SetValue<bool>("IsJump", message.IsJump);
        blackboard.SetValue<bool>("IsGrounded", message.IsGrounded);
    }
    public override void OnRemove()
    => RemoveListener<JumpStateMessage>(OnJumpStateSyn);
}
