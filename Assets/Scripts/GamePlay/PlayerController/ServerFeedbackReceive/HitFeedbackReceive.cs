using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class HitFeedbackReceive : BaseFeedbackReceive
{
    private ElementReceiver _elementReceiver;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        _elementReceiver = mainPlayer.GetComponent<ElementReceiver>();
        AddListener<PlayerShootHitMessage>(OnShootHit);
    }
    private void OnShootHit(PlayerShootHitMessage message)
    {
        if (message == null)
            return;
        // Debug.Log("【玩家受击】");
        if (!ElementUtility.TryToElementType(message.ElementAttack.ElementType, out var element))
            return;
        var attack = message.ElementAttack;
        _elementReceiver?.ReceiveElement(element, attack.Content, attack.Damage, attack.FromPlayerId);

        (var hitDir, _) = message.Dir;
        HitDir.ShowHitDir(-hitDir, mainPlayer.transform.forward);
    }
    public override void OnRemove()
    => RemoveListener<PlayerShootHitMessage>(OnShootHit);
}
