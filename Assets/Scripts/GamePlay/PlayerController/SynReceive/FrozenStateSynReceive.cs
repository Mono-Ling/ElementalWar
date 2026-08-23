using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class FrozenStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<FrozenStateMessage>(OnFrozen);
    }
    public override void OnRemove()
    => RemoveListener<FrozenStateMessage>(OnFrozen);
    private void OnFrozen(FrozenStateMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
            blackboard.SetValue("IsFrozen", message.IsFrozen);
        else
            Debug.LogWarning($"【冰冻状态同步接收器】不存在玩家{message.PlayerId}");
    }
}
