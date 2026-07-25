using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ShootStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<ShootStateMessage>(OnShootStateSyn);
    }
    private void OnShootStateSyn(ShootStateMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
            blackboard.SetValue<bool>("IsShoot", message.IsShoot);
        else
            Debug.LogWarning($"【射击状态同步接收器】不存在玩家{message.PlayerId}");
    }
    public override void OnRemove()
    => RemoveListener<ShootStateMessage>(OnShootStateSyn);
}
