using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 元素反应对照表
/// </summary>
[CreateAssetMenu(fileName = "NewElementReactionMap", menuName = "Element/ElementReactionMap")]
public class ElementReactionMap : ScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// 序列化中间结构：将 Key-Value 对打包为可序列化条目。
    /// </summary>
    [Serializable]
    public struct ElementReactionEntry
    {
        /// <summary>
        /// 元素组合 Key（支持 Flags 多选，如 Fire | Water）
        /// </summary>
        public ElementType key;

        /// <summary>
        /// 多态反应实例
        /// </summary>
        [SerializeReference]
        public BaseElementReaction reaction;
    }

    [SerializeField]
    private List<ElementReactionEntry> _serializedReactions = new();
    private Dictionary<ElementType, BaseElementReaction> _reactionMap = new();

    /// <summary>
    /// 尝试获取指定元素组合对应的反应。
    /// </summary>
    /// <param name="elementType">元素组合 Key</param>
    /// <param name="reaction">输出的反应实例，未找到时为 null</param>
    /// <returns>是否找到</returns>
    public bool TryGetReaction(ElementType elementType, out BaseElementReaction reaction)
    {
        return _reactionMap.TryGetValue(elementType, out reaction);
    }

    /// <summary>
    /// 获取所有已注册的反应条目。
    /// </summary>
    public IEnumerable<KeyValuePair<ElementType, BaseElementReaction>> GetReactions()
    {
        return _reactionMap;
    }
    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        _reactionMap = new Dictionary<ElementType, BaseElementReaction>();
        foreach (var entry in _serializedReactions)
        {
            if (entry.key == ElementType.None || entry.reaction == null)
                continue;

            if (_reactionMap.ContainsKey(entry.key))
            {
                Debug.LogWarning($"【元素反应对照表】重复的 Key: {entry.key}，将跳过重复条目");
                continue;
            }

            _reactionMap.Add(entry.key, entry.reaction);
        }
    }
}
