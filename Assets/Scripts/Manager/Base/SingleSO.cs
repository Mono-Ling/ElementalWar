using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleSO<T> : ScriptableObject where T : Object
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<T>(nameof(T).ToString());
                if (_instance == null)
                    Debug.LogError("【SO单例加载失败】" + typeof(T));
            }
            return _instance;
        }
    }
}
