using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class DeathStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<DeathStateMessage>(OnDeath);
    }
    public override void OnRemove()
    => RemoveListener<DeathStateMessage>(OnDeath);
    private void OnDeath(DeathStateMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
            blackboard?.SetValue("IsDeath", message.IsDeath);
    }
}
