using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasePoolItem
{
    protected int maxCount;
}
public class PoolItem<T> : BasePoolItem where T : class
{
    public int Count => _queue.Count;
    private Queue<T> _queue = new();
    public PoolItem(int count) => maxCount = count;
    public T Get()
    {
        if (_queue.TryDequeue(out var obj))
            return obj;
        else
            return null;
    }
    /// <summary>
    /// 放入桶
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <returns>是否有剩余空间</returns>
    public bool Put(T obj)
    {
        if (_queue.Count >= maxCount)
            return false;
        _queue.Enqueue(obj);
        return true;
    }
}
