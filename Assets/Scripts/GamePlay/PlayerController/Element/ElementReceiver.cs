using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ElementBuffSet))]
public class ElementReceiver : MonoBehaviour, IAutoInject<Blackboard>
{
    [Header("元素反应对照表")]
    public ElementReactionMap elementReactionMap;
    [Header("元素反应优先级表")]
    public ReactionPriorityTable reactionPriorityTable;
    private Blackboard _blackboard;
    private ElementAttachment _attachment;
    private ElementBuffSet _elementBuffSet;

    private DynamicTextCreator _dynamicTextCreator;
    void Awake()
    {
        if (elementReactionMap == null)
            Debug.LogError("【元素接收器】元素反应对照表为空");
        if (reactionPriorityTable == null)
            Debug.LogError("【元素接收器】元素反应优先级表为空");

        _elementBuffSet = GetComponent<ElementBuffSet>();
        if (_elementBuffSet == null)
            Debug.LogError("【元素接收器】元素Buff集合组件获取失败");

        _dynamicTextCreator = GetComponent<DynamicTextCreator>();
        if (_dynamicTextCreator == null)
            Debug.LogError("【元素接收器】动态文本UI创建组件获取失败");
    }
    public void AutoInject(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【元素接收器】传入黑板为空");
            return;
        }
        _blackboard = blackboard;

        foreach (var item in elementReactionMap.GetReactions())
            item.Value.Init(this, _elementBuffSet, _blackboard);
    }
    private bool TryGetAttachment(out ElementAttachment attachment)
    {
        if (_attachment == null)
            _blackboard.GetValue<ElementAttachment>("ElementAttachment", out _attachment);
        attachment = _attachment;
        if (attachment == null)
            Debug.LogError("【元素接收器】元素附着组件获取失败");
        return attachment != null;
    }
    public void ReceiveElement(ElementType elementType, float content)
    {
        if (elementType == ElementType.None || content <= 0)
            return;
        if (!TryGetAttachment(out var attachment))
            return;
        if (attachment.TotalElement.HasFlag(elementType))
        {
            attachment.AddElementContent(elementType, content);
            return;
        }

        var afterContent = content;

        if (!reactionPriorityTable.TryGetPriorityTable(elementType, out var elementPriorityList))
            Debug.LogWarning($"【元素接收器】不存在{elementType}的反应优先级列表");

        foreach (var beforeElement in elementPriorityList)
        {
            if (afterContent <= 0)
                break;
            if (beforeElement == ElementType.None || !attachment.TotalElement.HasFlag(beforeElement))
                continue;
            if (!attachment.elementContentDic.TryGetValue(beforeElement, out var beforeContent) || beforeContent <= 0)
                continue;

            var group = beforeElement | elementType;
            if (!elementReactionMap.TryGetReaction(group, out var reaction))
                continue;

            var delta = beforeContent;
            if (!reaction.OnReaction(beforeElement, elementType, ref beforeContent, ref afterContent))
                continue;
            beforeContent = Mathf.Max(beforeContent, 0);
            delta -= beforeContent;

            attachment.ReduceElementContent(beforeElement, delta);
            Debug.Log($"【元素接收器】触发反应{reaction.name}");

            _dynamicTextCreator?.ShowTextUI(reaction.name, reaction.color);
        }

        if (afterContent > 0)
        {
            attachment.AddNewElementType(elementType, afterContent);
            _elementBuffSet?.OnElementTrigger(elementType);
        }
    }
}
