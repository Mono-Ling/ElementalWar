using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleMono<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject(nameof(T));
                DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<T>();
            }
            return _instance;
        }
    }
}
