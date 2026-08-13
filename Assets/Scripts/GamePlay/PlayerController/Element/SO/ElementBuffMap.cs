using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewElementBuffMap", menuName = "Element/ElementBuffMap（元素Buff注册表）")]
public class ElementBuffMap : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    [SerializeReference]
    private List<BaseElementBuff> _serializeList = new();
    private Dictionary<Type, BaseElementBuff> _buffDic = new();

    public void OnAfterDeserialize()
    {
        _serializeList = _serializeList ?? new();
        _buffDic.Clear();
        foreach (var buff in _serializeList)
        {
            if (buff == null)
                continue;
            var type = buff.GetType();
            if (_buffDic.ContainsKey(type))
            {
                Debug.LogWarning($"【元素Buff映射表】{type}重复注册");
                continue;
            }
            _buffDic.Add(type, buff);
        }
    }
    public void OnBeforeSerialize() { }
    public bool TryGetElementBuff<T>(out T buff) where T : BaseElementBuff
    {
        buff = default;
        if (_buffDic.TryGetValue(typeof(T), out var baseBuff) && baseBuff is T findBuff)
        {
            buff = findBuff;
            return true;
        }
        return false;
    }
}