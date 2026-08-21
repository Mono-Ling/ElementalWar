using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ElementShieldStateSynReceive : BaseSynReceive
{
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<ElementShieldViewStateMessage>(OnElementShieldReceiive);
    }
    public override void OnRemove()
    => RemoveListener<ElementShieldViewStateMessage>(OnElementShieldReceiive);
    private void OnElementShieldReceiive(ElementShieldViewStateMessage message)
    {
        if (message == null || !ElementUtility.TryToElementType(message.ElementType, out var element))
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
            blackboard.SetValue("ElementShieldType", element);
        else
            Debug.LogWarning($"【跳跃状态同步接收器】不存在玩家{message.PlayerId}");
    }
}
