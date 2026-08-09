using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementListener
{
    private Dictionary<ElementType, Action<ElementType>> _listenerDic = new();
    public void Clear() => _listenerDic.Clear();
    public void AddListener(ElementType elementType, Action<ElementType> action)
    {
        if (_listenerDic.TryGetValue(elementType, out var actionItem))
            _listenerDic[elementType] += action;
        else
            _listenerDic.Add(elementType, action);
    }
    public void RemoveListener(ElementType elementType, Action<ElementType> action)
    {
        if (_listenerDic.TryGetValue(elementType, out var actionItem))
        {
            _listenerDic[elementType] -= action;

            if (_listenerDic[elementType] == null)
                _listenerDic.Remove(elementType);
        }
        else
            Debug.LogWarning($"【元素监听器】不存在{elementType}的监听");
    }
    public void Trigger(ElementType elementType)
    {
        if (_listenerDic.TryGetValue(elementType, out var actionItem))
            actionItem?.Invoke(elementType);
        // else
        //     Debug.LogWarning($"【元素监听器】不存在{elementType}的监听");
    }
}
