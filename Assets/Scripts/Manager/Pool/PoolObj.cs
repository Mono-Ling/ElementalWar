using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObj : MonoBehaviour
{
    public MonoPoolItem poolItem;
    public bool PutObj()
    {
        if (poolItem == null || !poolItem.Enable)
            return false;

        return poolItem.Put(gameObject);
    }
}
