using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ElementAttachmentSend : BaseSynSend
{
    private BlackboardArg<ElementAttachment> _elementAttachmentArg;
    private ElementAttachmentMessage _elementAttMes = new();
    public override void Init(Blackboard blackboard)
    {
        base.Init(blackboard);
        if (!blackboard.GetBlackboardArg("ElementAttachment", out _elementAttachmentArg))
        {
            Debug.LogError("【元素附着同步发送】元素附着器组件黑板参数获取失败");
            return;
        }
        _elementAttachmentArg.OnValueChange += OnElementAttachmentChange;
        SetHeader(true);
        OnElementAttachmentChange(_elementAttachmentArg.value);
    }
    private void OnElementAttachmentChange(ElementAttachment attachment)
    {
        if (attachment == null)
            return;
        _elementAttMes.ElementAttachmentMap.Clear();
        foreach (var item in attachment.ElementContentDic)
            _elementAttMes.ElementAttachmentMap.Add(ElementUtility.ToNumber(item.Key), item.Value);

        Send(_elementAttMes);
    }
    public override void OnRemove()
    => _elementAttachmentArg.OnValueChange -= OnElementAttachmentChange;
}
