using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class RemoteStateSynTest : MonoBehaviour
{
    public StaticSceneAsset sceneAsset;
    // Start is called before the first frame update
    void Start()
    {
        // var managedPlayerMgr = ManagedPlayerMgr.Instance;
        // PlayerRegistryMes mes = new();
        // mes.PlayerList.Add(0);
        // EventBus.Instance.Trigger<NetPackage>(EventType.OnReceive, new(mes));
        var managedMgr = ManagedPlayerMgr.Instance;
        var dynamicMgr = DynamicSceneItemMgr.Instance;
        StaticSceneManager.Instance.LoadWall(sceneAsset);
        NetManager.Instance.StartClient();
        Cursor.lockState = CursorLockMode.Confined;
    }
}
