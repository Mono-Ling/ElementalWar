using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementAttachmentInfo
{
    public IReadOnlyDictionary<ElementType, float> AttachmentInfo
    => _elementAttachmentDic;
    private Dictionary<ElementType, float> _elementAttachmentDic = new();

    private List<(ElementType element, float content)> _tempList = new();
    public ElementAttachmentInfo()
    {
        var array = Enum.GetValues(typeof(ElementType));
        foreach (var item in array)
        {
            if (item is not ElementType element || element == ElementType.None)
                continue;
            _elementAttachmentDic.Add(element, default);
        }
    }
    public void UpdateElementAttachment(IReadOnlyDictionary<ElementType, float> elementDic)
    {
        if (elementDic == null)
            return;
        _tempList.Clear();
        foreach (var element in _elementAttachmentDic.Keys)
        {
            if (elementDic.TryGetValue(element, out var content))
                _tempList.Add((element, content));
            else
                _tempList.Add((element, default));
        }

        foreach (var item in _tempList)
            _elementAttachmentDic[item.element] = item.content;
    }
}
