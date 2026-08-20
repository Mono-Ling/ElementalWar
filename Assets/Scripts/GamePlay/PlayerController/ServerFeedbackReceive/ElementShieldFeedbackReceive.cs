using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ElementShieldFeedbackReceive : BaseFeedbackReceive
{
    private ElementBuffSet _elementBuffSet;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);

        _elementBuffSet = mainPlayer.GetComponent<ElementBuffSet>();
        if (_elementBuffSet == null)
            Debug.LogError("【主玩家元素护盾反馈接收器】元素Buff集合组件获取失败");

        AddListener<PlayerElementShieldMessage>(OnElementShieldReceive);
    }
    public override void OnRemove()
    => RemoveListener<PlayerElementShieldMessage>(OnElementShieldReceive);
    private void OnElementShieldReceive(PlayerElementShieldMessage message)
    {
        if (message == null)
            return;
        if (!ElementUtility.TryToElementType(message.ElementType, out var element))
            return;

        if (_elementBuffSet.TryGetElementBuff<ElementShieldBuff>(out var shield))
            shield.InitElementShield(element);
        else
            _elementBuffSet.AddElementBuff<ElementShieldBuff>()?.InitElementShield(element);

        Debug.Log($"【主玩家元素护盾反馈接收器】玩家添加{element}元素护盾");
    }
}
