using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ExpHitFeedbackReceive : BaseFeedbackReceive
{
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        AddListener<PlayerExpHitMessage>(OnExpHit);
    }
    private void OnExpHit(PlayerExpHitMessage message)
    {
        if (message == null)
            return;
        Debug.Log("【玩家受爆炸波及】");
    }
    public override void OnRemove()
    => RemoveListener<PlayerExpHitMessage>(OnExpHit);
}
