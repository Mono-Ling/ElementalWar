using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ElementShieldStateSynSend : BaseSynSend
{
    private BlackboardArg<ElementType> _elementShieldTypeArg;
    private ElementShieldViewStateMessage _viewStateMes = new();
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        if (blackboard.GetBlackboardArg("ElementShieldType", out _elementShieldTypeArg))
            _elementShieldTypeArg.OnValueChange += OnElementShieldChange;
        else
            Debug.LogError("【玩家元素护盾同步发送】护盾类型黑板参数获取失败");
        SetHeader(true);
    }
    public override void OnRemove()
    {
        if (_elementShieldTypeArg != null)
            _elementShieldTypeArg.OnValueChange -= OnElementShieldChange;
    }
    private void OnElementShieldChange(ElementType element)
    {
        _viewStateMes.ElementType = ElementUtility.ToNumber(element);
        Send(_viewStateMes);
    }
}
