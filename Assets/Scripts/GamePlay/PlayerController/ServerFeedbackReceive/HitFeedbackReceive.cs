using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class HitFeedbackReceive : BaseFeedbackReceive
{
    public override void Init(MainPlayer mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
    }
    private void OnShootHit(PlayerShootHitMessage message)
    {
        if (message == null)
            Debug.Log("【玩家受击】");
    }
    public override void OnRemove()
    => RemoveListener<PlayerShootHitMessage>(OnShootHit);
}
