using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoPoolItem
{
    public GameObject prefab { get; private set; }
    public GameObject rootObj { get; private set; }
    private Queue<GameObject> _queue = new();
    private int _maxCount;
    public MonoPoolItem(GameObject prefab, GameObject root, int maxCount)
    {
        this.prefab = prefab;
        this.rootObj = root;
        this._maxCount = maxCount;
    }
    public GameObject Get()
    {
        if (!_queue.TryDequeue(out var obj))
            return null;
        //GameObject obj = _queue.Dequeue();
        obj.SetActive(true);
        obj.transform.SetParent(null);
        return obj;
    }
    public bool Put(GameObject obj)
    {
        if (_queue.Count >= _maxCount)
            return false;
        obj.transform.SetParent(rootObj.transform);
        obj.SetActive(false);
        _queue.Enqueue(obj);
        return true;
    }
    public void ClearAll() => GameObject.Destroy(rootObj);
}
