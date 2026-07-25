using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class LocalStateSynTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var managedPlayerMgr = ManagedPlayerMgr.Instance;
        PlayerRegistryMes mes = new();
        mes.PlayerList.Add(0);
        EventBus.Instance.Trigger<NetPackage>(EventType.OnReceive, new(mes));
        EventBus.Instance.AddListener<NetPackage>(EventType.SendTo, OnLocalSend);
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.SendTo, OnLocalSend);
    }
    private void OnLocalSend(NetPackage package)
    {
        if (package.message == null)
        {
            Debug.LogWarning("【本地状态同步测试】无效消息");
            return;
        }
        if (package.message is PositionStateMessage posMes)
        {
            PlayerPosStateMesMap posMap = new();
            posMap.ServerTime = DateTime.UtcNow.Ticks;
            posMap.PlayerPosStateMap.Add(0, posMes);
            package = new(null, posMap);
        }
        EventBus.Instance.Trigger(EventType.OnReceive, package);
    }
}
