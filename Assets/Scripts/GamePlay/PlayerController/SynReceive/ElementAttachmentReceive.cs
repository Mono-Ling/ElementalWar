using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ElementAttachmentReceive : BaseSynReceive
{
    private ElementAttachmentInfo _elementAttachmentInfo = new();
    private Dictionary<ElementType, float> _tempDic = new();
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<ElementAttachmentMessage>(OnElementAttachment);
    }
    private void OnElementAttachment(ElementAttachmentMessage message)
    {
        if (message == null)
            return;
        if (blackboardDic.TryGetValue(message.PlayerId, out var blackboard))
        {
            _tempDic.Clear();
            foreach (var item in message.ElementAttachmentMap)
                if (ElementUtility.TryToElementType(item.Key, out var element))
                    _tempDic.Add(element, item.Value);

            _elementAttachmentInfo.UpdateElementAttachment(_tempDic);
            blackboard.SetValue("ElementAttachmentInfo", _elementAttachmentInfo, true);
        }
    }
    public override void OnRemove()
    => RemoveListener<ElementAttachmentMessage>(OnElementAttachment);
}
