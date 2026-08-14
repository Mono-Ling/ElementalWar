using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 元素附着组件：管理元素附着量与随时间衰减
/// </summary>
public class ElementAttachment : MonoBehaviour, IAutoInject<Blackboard>
{
    public IReadOnlyDictionary<ElementType, float> ElementContentDic
        => _elementContentDic;
    public ElementType TotalElement { get; private set; }
    private Dictionary<ElementType, float> _elementContentDic = new();
    private Dictionary<ElementType, Coroutine> _attenuationDic = new();
    private Blackboard _blackboard;

    public void AutoInject(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【元素附着】传入黑板为空");
            return;
        }
        _blackboard = blackboard;
        _blackboard.SetValue("ElementAttachment", this);
    }
    /// <summary>
    /// 新增元素附着
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <param name="content">元素量</param>
    public void AddNewElementType(ElementType elementType, float content)
    {
        if (elementType == ElementType.None || content <= 0)
            return;
        if (TotalElement.HasFlag(elementType))
            return;
        if (_attenuationDic.ContainsKey(elementType) || _elementContentDic.ContainsKey(elementType))
        {
            Debug.LogError("【元素附着】计数器重入");
            return;
        }
        TotalElement |= elementType;
        _elementContentDic.Add(elementType, content);

        var cor = StartCoroutine(ElementAttenuation(elementType));
        _attenuationDic.Add(elementType, cor);

        _blackboard.SetValue("AttachTotalElement", TotalElement);
        Debug.Log($"【元素附着】元素附着{elementType}|Total:{TotalElement}");

        _blackboard.SetValue("ElementAttachment", this, true);
    }
    private IEnumerator ElementAttenuation(ElementType elementType)
    {
        var element = elementType;
        WaitForSeconds waitForSeconds = new(ElementUtility.Content.ATTENUA_DELAY);
        while (TotalElement.HasFlag(element))
        {
            yield return waitForSeconds;
            ReduceElementContent(element, ElementUtility.Content.ATTENUA_SPEED);
        }
        _attenuationDic.Remove(element);
    }
    public void AddElementContent(ElementType elementType, float delta)
    {
        if (elementType == ElementType.None || delta <= 0)
            return;
        if (_elementContentDic.TryGetValue(elementType, out var content))
        {
            var curr = content + delta;
            _elementContentDic[elementType] = Mathf.Min(curr, ElementUtility.Content.STRONG);

            _blackboard.SetValue("ElementAttachment", this, true);
        }
        else
            Debug.LogWarning($"【元素附着】不存在{elementType}元素附着量计数器");
    }
    public void ReduceElementContent(ElementType elementType, float delta)
    {
        if (elementType == ElementType.None || delta <= 0)
            return;
        if (_elementContentDic.TryGetValue(elementType, out var content))
        {
            var curr = content - delta;
            if (curr <= 0)
            {
                _elementContentDic.Remove(elementType);
                TotalElement &= ~elementType;
            }
            else
                _elementContentDic[elementType] = curr;
            _blackboard.SetValue("AttachTotalElement", TotalElement);

            _blackboard.SetValue("ElementAttachment", this, true);
        }
        else
            Debug.LogWarning($"【元素附着】不存在{elementType}元素附着量计数器");
    }
    void OnDestroy()
    => StopAllCoroutines();
}
