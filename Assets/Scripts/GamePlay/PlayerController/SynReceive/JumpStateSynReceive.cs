using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class JumpStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<JumpStateMessage>(OnJumpStateSyn);
    }
    private void OnJumpStateSyn(JumpStateMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
        {
            blackboard.SetValue<bool>("IsJump", message.IsJump);
            blackboard.SetValue<bool>("IsGrounded", message.IsGrounded);
        }
        else
            Debug.LogWarning($"【跳跃状态同步接收器】不存在玩家{message.PlayerId}");
    }
    public override void OnRemove()
    => RemoveListener<JumpStateMessage>(OnJumpStateSyn);
}
