using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ThrowStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<ThrowStateMessage>(OnThrawStateSyn);
    }
    private void OnThrawStateSyn(ThrowStateMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
        {
            blackboard.SetValue<bool>("IsThrow", message.IsThrow);
            blackboard.SetValue<bool>("IsThrowFire", message.IsThrowFire);
        }
        else
            Debug.LogWarning($"【投掷状态同步接收器】不存在玩家{message.PlayerId}");
    }
    public override void OnRemove()
    => RemoveListener<ThrowStateMessage>(OnThrawStateSyn);
}
