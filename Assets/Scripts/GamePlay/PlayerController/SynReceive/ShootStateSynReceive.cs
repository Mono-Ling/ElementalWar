using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ShootStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Blackboard blackboard)
    {
        base.Init(otherPlayer, blackboard);
        AddListener<ShootStateMessage>(OnShootStateSyn);
    }
    private void OnShootStateSyn(ShootStateMessage message)
    {
        if (message == null)
            return;
        blackboard.SetValue<bool>("IsShoot", message.IsShoot);
    }
    public override void OnRemove()
    => RemoveListener<ShootStateMessage>(OnShootStateSyn);
}
