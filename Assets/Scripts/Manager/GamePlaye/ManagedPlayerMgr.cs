#define LOCALDEBUG
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ManagedPlayerMgr : SingleMono<ManagedPlayerMgr>
{
    private Dictionary<int, OtherPlayer> _managedPlayerDic = new();
    // Start is called before the first frame update
    void Start()
    {
#if LOCALDEBUG
        CreateManagedPlayer(-1);
        EventBus.Instance.AddListener<NetPackage>(EventType.SendTo, OnLocalStateSynMessage);
#else
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnRemoteStateSynMessage);
#endif
    }
    private void OnRemoteStateSynMessage(NetPackage package)
    {
        if (package.sendType != SendType.Udp || package.header == null)
            return;
        if (package.header is UdpHeader udpHeader)
        {
            if (_managedPlayerDic.TryGetValue(udpHeader.PlayerId, out var otherPlayer))
                otherPlayer.OnStateSynMessageReceive(package.message);
            else
                Debug.LogWarning($"【托管玩家管理器】不存在玩家{udpHeader.PlayerId}");
        }
        else
            Debug.LogWarning("【托管玩家管理器】无效UDP消息");
    }
#if LOCALDEBUG
    private void OnLocalStateSynMessage(NetPackage package)
    {
        if (package.sendType != SendType.Udp || package.header == null)
            return;
        if (_managedPlayerDic.TryGetValue(-1, out var otherPlayer))
            otherPlayer.OnStateSynMessageReceive(package.message);
        else
            Debug.LogWarning($"【托管玩家管理器】不存在玩家{-1}");
    }
#endif
    void OnDestroy()
    {
#if LOCALDEBUG
        EventBus.Instance.RemoveListener<NetPackage>(EventType.SendTo, OnLocalStateSynMessage);
#else
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnRemoteStateSynMessage);
#endif
    }
    private void CreateManagedPlayer(int id)
    {
        var playerViewObj = Instantiate(Resources.Load<GameObject>("Player"));
        var controller = playerViewObj.GetComponent<PlayerController>();

        var playerObj = Instantiate(Resources.Load<GameObject>("OtherPlayer"));
        playerObj.name = $"OtherPlayer_{id}";
        var otherPlayer = playerObj.GetComponent<OtherPlayer>();
        otherPlayer.InitOtherPlayer(id, controller);
        _managedPlayerDic.Add(id, otherPlayer);
    }
}
