using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ExpHitFeedbackReceive : BaseFeedbackReceive
{
    public float elementContent = ElementUtility.Content.STRONG;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        AddListener<PlayerExpHitMessage>(OnExpHit);
    }
    private void OnExpHit(PlayerExpHitMessage message)
    {
        if (message == null)
            return;
        if (!ElementUtility.TryToElementType(message.ElementType, out var element))
            return;
        if (blackboard.GetValue<ElementReceiver>("ElementReceiver", out var elementReceiver))
            elementReceiver.ReceiveElement(element, elementContent);
        Debug.Log($"【玩家受爆炸波及】{element}");
    }
    public override void OnRemove()
    => RemoveListener<PlayerExpHitMessage>(OnExpHit);
}
