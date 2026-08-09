using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewReactionPriorityTable", menuName = "Element/ReactionPriorityTable")]
public class ReactionPriorityTable : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    public class PriorityInfo
    {
        public ElementType afterElement;
        public List<ElementType> seqList = new();
    }
    public List<PriorityInfo> priorityInfos = new();
    private Dictionary<ElementType, List<ElementType>> _priorityDic = new();
    public void OnAfterDeserialize()
    {
        _priorityDic.Clear();
        foreach (var info in priorityInfos)
        {
            if (_priorityDic.ContainsKey(info.afterElement))
            {
                Debug.LogWarning($"【反应优先级表】{info.afterElement}优先级重复注册");
                continue;
            }
            info.seqList ??= new();
            _priorityDic.Add(info.afterElement, info.seqList);
        }
    }
    public void OnBeforeSerialize() { }
    public bool TryGetPriorityTable(ElementType elementType, out IReadOnlyList<ElementType> priorityList)
    {
        priorityList = Array.Empty<ElementType>();
        if (!_priorityDic.TryGetValue(elementType, out var list))
            return false;
        priorityList = list;
        return true;
    }
}