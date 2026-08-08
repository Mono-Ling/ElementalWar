using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementInfoMap", menuName = "Element/ElementInfoMap")]
public class ElementInfoMap : SingleSO<ElementInfoMap>, ISerializationCallbackReceiver
{
    public List<ElementInfo> infoList = new();
    public HashSet<ElementInfo> elementInfos { get; private set; } = new();
    public bool TryGetElementInfo(ElementType elementType, out ElementInfo info)
    {
        var key = new ElementInfo { elementType = elementType };
        return elementInfos.TryGetValue(key, out info);
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        elementInfos.Clear();
        foreach (var info in infoList)
            if (!elementInfos.Add(info))
                Debug.LogWarning("【元素信息对照表】重复的元素类型");
    }
}
[Serializable]
public struct ElementInfo : IEquatable<ElementInfo>
{
    public ElementType elementType;
    public string name;
    public Color color;

    public override bool Equals(object obj)
    => obj is ElementInfo other && Equals(other);

    public bool Equals(ElementInfo other)
    => elementType == other.elementType;
    public override int GetHashCode()
    => elementType.GetHashCode();
}