using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Single<ObjectPool>
{
    private const int DEFAULT_COUNT = 10;
    private Dictionary<Type, BasePoolItem> _objectDic = new();
    public void CreatePool<T>(int maxCount, int initCount = 0) where T : class, new()
    {
        Type type = typeof(T);
        if (_objectDic.ContainsKey(type))
        {
            Debug.LogWarning($"【对象池】{type}对象池已存在");
            return;
        }
        PoolItem<T> newPoolItem = new(maxCount);
        for (int i = 0; i < initCount; i++)
            newPoolItem.Put(new T());
        _objectDic.Add(type, newPoolItem);
    }
    public T GetObject<T>(Action<T> init = null) where T : class, new()
    {
        T obj = null;
        if (_objectDic.TryGetValue(typeof(T), out var baseItem) && baseItem is PoolItem<T> item)
            obj = item.Get();
        obj ??= new T();
        init?.Invoke(obj);
        return obj;
    }
    public void PutObject<T>(T obj, Action<T> reset = null) where T : class, new()
    {
        reset?.Invoke(obj);
        if (_objectDic.TryGetValue(typeof(T), out var baseItem) && baseItem is PoolItem<T> item)
            item.Put(obj);
        else
        {
            CreatePool<T>(DEFAULT_COUNT);
            (_objectDic[typeof(T)] as PoolItem<T>).Put(obj);
        }
    }
    public void ClearPool<T>()
    {
        var type = typeof(T);
        if (_objectDic.ContainsKey(type))
            _objectDic.Remove(type);
        else
            Debug.LogWarning($"【对象池】不存在{type}的对象池");
    }
    public void ClearAllPool() => _objectDic.Clear();
}
