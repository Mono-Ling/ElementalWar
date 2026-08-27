using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        var dynamicMgr = DynamicSceneItemMgr.Instance;
        NetManager.Instance.StartClient();
        Cursor.lockState = CursorLockMode.Confined;
    }
}
