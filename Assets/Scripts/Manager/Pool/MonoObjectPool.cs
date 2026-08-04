using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoObjectPool : Single<MonoObjectPool>
{
    private const int DEFAULT_COUNT = 10;
    private static int _poolId;
    private Dictionary<string, MonoPoolItem> _objPoolDic = new();
    public void CreatePool(string path, int maxCount, int initCount = 0)
    {
        if (_objPoolDic.ContainsKey(path))
        {
            Debug.LogWarning($"【Mono对象池】{path}对象池已存在");
            return;
        }
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"【Mono对象池】{path}加载失败");
            return;
        }
        GameObject root = new GameObject(path);
        MonoPoolItem item = new(prefab, root, maxCount);
        _objPoolDic.Add(path, item);
        for (int i = 0; i < initCount; i++)
            item.Put(CreateObject(path));
    }
    public GameObject GetObject(string path, Action<GameObject> init = null)
    {
        GameObject obj = null;
        if (_objPoolDic.TryGetValue(path, out var item))
            obj = item.Get();
        if (obj == null)
        {
            if (!_objPoolDic.ContainsKey(path))
                CreatePool(path, DEFAULT_COUNT);
            obj = CreateObject(path);
        }
        if (obj != null)
            init?.Invoke(obj);
        return obj;
    }
    public void PutObject(GameObject obj, Action<GameObject> reset = null)
    {
        if (obj == null) return;
        reset?.Invoke(obj);
        var poolObj = obj.GetComponent<PoolObj>();
        if (poolObj == null)
        {
            Debug.LogWarning("【Mono对象池】PoolObj获取失败");
            GameObject.Destroy(obj);
            return;
        }
        if (!poolObj.PutObj())
            GameObject.Destroy(obj);
    }
    public void ClearPool(string path)
    {
        if (_objPoolDic.TryGetValue(path, out var item))
        {
            item.ClearAll();
            _objPoolDic.Remove(path);
        }
        else
            Debug.LogWarning($"【Mono对象池】{path}对象池不存在");
    }
    public void ClearAll()
    {
        foreach (var item in _objPoolDic.Values)
            item.ClearAll();
        _objPoolDic.Clear();
    }
    private GameObject CreateObject(string path)
    {
        if (_objPoolDic.TryGetValue(path, out var item))
        {
            if (item.prefab == null)
            {
                Debug.LogError($"【Mono对象池】{path}预制体为空");
                return null;
            }
            var obj = GameObject.Instantiate(item.prefab);
            // obj.name = path;
            var poolObj = obj.GetComponent<PoolObj>();
            if (poolObj == null)
            {
                Debug.LogWarning("【Mono对象池】PoolObj获取失败");
                return obj;
            }
            poolObj.poolItem = item;
            return obj;
        }
        else
            return null;
    }
}
