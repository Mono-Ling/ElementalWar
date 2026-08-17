using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementListener
{
    private Dictionary<ElementType, Action<ElementType>> _listenerDic = new();

    private Dictionary<ElementType, RefAction<float, int>> _attackListenerDic = new();
    public void Clear()
    {
        _listenerDic.Clear();
        _attackListenerDic.Clear();
    }
    #region 元素附着监听
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
    #endregion
    #region 元素攻击监听（用于伤害减免）
    /// <summary>
    /// 注册元素攻击监听
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <param name="action">（元素量，元素伤害）委托</param>
    public void AddAttackListener(ElementType elementType, RefAction<float, int> action)
    {
        if (_attackListenerDic.TryGetValue(elementType, out var actionItem))
            _attackListenerDic[elementType] += action;
        else
            _attackListenerDic.Add(elementType, action);
    }
    /// <summary>
    /// 注销元素攻击监听
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <param name="action">（元素量，元素伤害）委托</param>
    public void RemoveAttackListener(ElementType elementType, RefAction<float, int> action)
    {
        if (_attackListenerDic.TryGetValue(elementType, out var actionItem))
        {
            _attackListenerDic[elementType] -= action;

            if (_attackListenerDic[elementType] == null)
                _attackListenerDic.Remove(elementType);
        }
        else
            Debug.LogWarning($"【元素监听器】不存在{elementType}的监听");
    }
    /// <summary>
    /// 触发元素伤害监听
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <param name="elementContent">元素量</param>
    /// <param name="elementDamage">元素伤害</param>
    public void TriggerAttackListener(
        ElementType elementType,
        ref float elementContent, ref int elementDamage)
    {
        if (_attackListenerDic.TryGetValue(elementType, out var action))
            action?.Invoke(ref elementContent, ref elementDamage);
    }
    #endregion
}
