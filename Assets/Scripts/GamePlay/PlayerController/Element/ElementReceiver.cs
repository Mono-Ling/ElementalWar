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

    private MainPlayerHP _mainPlayerHP;

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

        _mainPlayerHP = GetComponent<MainPlayerHP>();
        if (_mainPlayerHP == null)
            Debug.LogError("【元素接收器】主玩家生命值组件获取失败");
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
    public void ReceiveElement(ElementType elementType, float content, int damage, int attackFrom = MainPlayerHP.DEFAULT_PLAYER_ID)
    {
        if (elementType == ElementType.None || content <= 0)
            return;
        if (!TryGetAttachment(out var attachment))
            return;

        _mainPlayerHP?.SetAttackFrom(attackFrom);

        var afterContent = content;
        var elementDamage = damage;

        // 触发元素攻击监听，用于伤害减免效果
        _elementBuffSet?.OnElementAttackTrigger(elementType, ref afterContent, ref elementDamage);
        afterContent = Mathf.Max(afterContent, 0);
        elementDamage = Mathf.Max(elementDamage, 0);

        if (!reactionPriorityTable.TryGetPriorityTable(elementType, out var elementPriorityList))
            Debug.LogWarning($"【元素接收器】不存在{elementType}的反应优先级列表");

        foreach (var beforeElement in elementPriorityList)
        {
            if (afterContent <= 0)
                break;
            if (!attachment.TotalElement.HasFlag(beforeElement))
                continue;
            if (!attachment.ElementContentDic.TryGetValue(beforeElement, out var beforeContent) || beforeContent <= 0)
                continue;

            var group = beforeElement | elementType;
            if (!elementReactionMap.TryGetReaction(group, out var reaction))
                continue;

            var beforeDelta = beforeContent;
            var afterDelta = afterContent;
            if (!reaction.OnReaction(beforeElement, elementType,
                ref beforeContent, ref afterContent))
                continue;
            beforeContent = Mathf.Max(beforeContent, 0);
            afterContent = Mathf.Max(afterContent, 0);
            beforeDelta -= beforeContent;
            afterDelta -= afterContent;

            var reactionDamage = reaction.GetDamage(elementType, elementDamage);

            attachment.ReduceElementContent(beforeElement, beforeDelta);
            _mainPlayerHP?.ReduceHP(reactionDamage, reaction.color);
            Debug.Log($"【元素接收器】触发反应{reaction.name}");

            _dynamicTextCreator?.ShowTextUI(reaction.name, reaction.color);
        }

        if (afterContent > 0)
        {
            if (attachment.TotalElement.HasFlag(elementType))
                attachment.AddElementContent(elementType, afterContent);
            else
                attachment.AddNewElementType(elementType, afterContent);

            float damageNum = Mathf.Clamp01(afterContent / content);
            elementDamage = Mathf.CeilToInt(elementDamage * damageNum);
            _mainPlayerHP?.ElementDamage(elementDamage, elementType);
            _elementBuffSet?.OnElementTrigger(elementType);
        }

        _mainPlayerHP.SetAttackFrom();
    }
}
