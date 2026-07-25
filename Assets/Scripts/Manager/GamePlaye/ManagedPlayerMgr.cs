using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ManagedPlayerMgr : SingleMono<ManagedPlayerMgr>
{
    private OtherPlayer _otherPlayer;
    private List<GameObject> _playerObjList = new();
    // Start is called before the first frame update
    void Awake()
    {
        var playerObj = Instantiate(Resources.Load<GameObject>("OtherPlayer"));
        playerObj.name = "OtherPlayer";
        _otherPlayer = playerObj.GetComponent<OtherPlayer>();

        _otherPlayer.transform.SetParent(transform);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnPlayerRegistryMes);
    }
    void OnDestroy()
    {
        Clear();
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnPlayerRegistryMes);
    }
    private void OnPlayerRegistryMes(NetPackage package)
    {
        if (package.sendType != SendType.Tcp || package.message is not PlayerRegistryMes mes)
            return;
        CreateManagedPlayer(mes);
    }
    private void CreateManagedPlayer(PlayerRegistryMes mes)
    {
        Clear();

        Dictionary<int, PlayerController> playerDic = new();
        foreach (int id in mes.PlayerList)
        {
            if (playerDic.ContainsKey(id))
            {
                Debug.LogWarning($"【托管玩家管理器】玩家{id}重复注册");
                continue;
            }
            var playerViewObj = MonoObjectPool.Instance.GetObject("Player");
            var controller = playerViewObj.GetComponent<PlayerController>();
            playerDic.Add(id, controller);

            _playerObjList.Add(playerViewObj);
        }
        _otherPlayer.InitOtherPlayer(playerDic);
    }
    private void Clear()
    {
        foreach (var obj in _playerObjList)
            MonoObjectPool.Instance.PutObject(obj);
        _playerObjList.Clear();
    }
}