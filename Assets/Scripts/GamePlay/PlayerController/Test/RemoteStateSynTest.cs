using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class RemoteStateSynTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // var managedPlayerMgr = ManagedPlayerMgr.Instance;
        // PlayerRegistryMes mes = new();
        // mes.PlayerList.Add(0);
        // EventBus.Instance.Trigger<NetPackage>(EventType.OnReceive, new(mes));
        var managedMgr = ManagedPlayerMgr.Instance;
        NetManager.Instance.StartClient();
    }
}
