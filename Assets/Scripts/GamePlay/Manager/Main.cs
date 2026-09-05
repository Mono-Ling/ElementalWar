using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        var dynamicMgr = DynamicSceneItemMgr.Instance;
        NetManager.Instance.StartClient();
        // #if UNITY_EDITOR
        //         Cursor.lockState = CursorLockMode.Confined;
        // #endif
    }
}
