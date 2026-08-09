using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementReceiver : MonoBehaviour, IAutoInject<Blackboard>
{
    public ElementType TotalElement { get; private set; }
    public Dictionary<ElementType, float> elementContentDic { get; private set; } = new();
    [Header("元素反应对照表")]
    public ElementReactionMap elementReactionMap;
    [Header("元素反应优先级表")]
    public ReactionPriorityTable reactionPriorityTable;
    private Dictionary<ElementType, Coroutine> _attenuationDic = new();
    private Blackboard _blackboard;

    private BlackboardArg<ElementBuffSet> _elementBuffSetArg;
    void Awake()
    {
        if (elementReactionMap == null)
            Debug.LogError("【元素接收器】元素反应对照表为空");
        if (reactionPriorityTable == null)
            Debug.LogError("【元素接收器】元素反应优先级表为空");
    }
    public void AutoInject(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【元素接收器】传入黑板为空");
            return;
        }
        _blackboard = blackboard;
        _blackboard.SetValue("ElementReceiver", this);

        foreach (var item in elementReactionMap.GetReactions())
            item.Value.Init(this, _blackboard);
    }
    /// <summary>
    /// 新增元素附着
    /// </summary>
    /// <param name="elementType">元素类型</param>
    /// <param name="content">元素量</param>
    private void AddNewElementType(ElementType elementType, float content)
    {
        if (elementType == ElementType.None || content <= 0)
            return;
        if (TotalElement.HasFlag(elementType))
            return;
        if (_attenuationDic.ContainsKey(elementType) || elementContentDic.ContainsKey(elementType))
        {
            Debug.LogError("【元素反应接收器】计数器重入");
            return;
        }
        TotalElement |= elementType;
        elementContentDic.Add(elementType, content);

        var cor = StartCoroutine(ElementAttenuation(elementType));
        _attenuationDic.Add(elementType, cor);

        _blackboard.SetValue("AttachTotalElement", TotalElement);
        Debug.Log($"【元素反应接收器】元素附着{elementType}|Total:{TotalElement}");
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
        if (elementContentDic.TryGetValue(elementType, out var content))
        {
            var curr = content + delta;
            elementContentDic[elementType] = Mathf.Min(curr, ElementUtility.Content.STRONG);
        }
        else
            Debug.LogWarning($"【元素接收器】不存在{elementType}元素附着量计数器");
    }
    public void ReduceElementContent(ElementType elementType, float delta)
    {
        if (elementType == ElementType.None || delta <= 0)
            return;
        if (elementContentDic.TryGetValue(elementType, out var content))
        {
            var curr = content - delta;
            if (curr <= 0)
            {
                elementContentDic.Remove(elementType);
                TotalElement &= ~elementType;
            }
            else
                elementContentDic[elementType] = curr;
            _blackboard.SetValue("AttachTotalElement", TotalElement);
        }
        else
            Debug.LogWarning($"【元素接收器】不存在{elementType}元素附着量计数器");
    }
    public void ReceiveElement(ElementType elementType, float content)
    {
        if (elementType == ElementType.None || content <= 0)
            return;
        if (TotalElement.HasFlag(elementType))
        {
            AddElementContent(elementType, content);
            return;
        }

        var afterContent = content;

        if (!reactionPriorityTable.TryGetPriorityTable(elementType, out var elementPriorityList))
            Debug.LogWarning($"【元素接收器】不存在{elementType}的反应优先级列表");

        foreach (var beforeElement in elementPriorityList)
        {
            if (afterContent <= 0)
                break;
            if (beforeElement == ElementType.None || !TotalElement.HasFlag(beforeElement))
                continue;
            if (!elementContentDic.TryGetValue(beforeElement, out var beforeContent) || beforeContent <= 0)
                continue;

            var group = beforeElement | elementType;
            if (!elementReactionMap.TryGetReaction(group, out var reaction))
                continue;

            var delta = beforeContent;
            reaction.OnReaction(beforeElement, elementType, ref beforeContent, ref afterContent);
            beforeContent = Mathf.Max(beforeContent, 0);
            delta -= beforeContent;

            ReduceElementContent(beforeElement, delta);
            Debug.Log($"【元素接收器】触发反应{reaction.name}");
        }

        if (afterContent > 0)
        {
            AddNewElementType(elementType, afterContent);

            if (_elementBuffSetArg == null)
                _blackboard.GetBlackboardArg("ElementBuffSet", out _elementBuffSetArg);
            _elementBuffSetArg?.value?.OnElementTrigger(elementType);
        }
    }
    void OnDestroy()
    => StopAllCoroutines();
}