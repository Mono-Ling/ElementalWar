using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ExpHitFeedbackReceive : BaseFeedbackReceive
{
    private ElementReceiver _elementReceiver;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        _elementReceiver = mainPlayer.GetComponent<ElementReceiver>();
        AddListener<PlayerExpHitMessage>(OnExpHit);
    }
    private void OnExpHit(PlayerExpHitMessage message)
    {
        if (message == null)
            return;
        if (!ElementUtility.TryToElementType(message.ElementAttack.ElementType, out var element))
            return;
        var attack = message.ElementAttack;
        _elementReceiver?.ReceiveElement(element, attack.Content, attack.Damage, attack.FromPlayerId);
        Debug.Log($"【玩家受爆炸波及】{element}");
    }
    public override void OnRemove()
    => RemoveListener<PlayerExpHitMessage>(OnExpHit);
}
