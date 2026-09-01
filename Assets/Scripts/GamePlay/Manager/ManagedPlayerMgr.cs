// #define LOCALDEBUG
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class ManagedPlayerMgr : SingleMono<ManagedPlayerMgr>
{
    private OtherPlayer _otherPlayer;
    private List<GameObject> _playerObjList = new();
    void OnDestroy()
    => StopManagedPlayer();
    public void StartManagedPlayer()
    {
        var playerObj = Instantiate(Resources.Load<GameObject>("OtherPlayer"));
        playerObj.name = "OtherPlayer";
        _otherPlayer = playerObj.GetComponent<OtherPlayer>();

        _otherPlayer.transform.SetParent(transform);
    }
    public void StopManagedPlayer()
    {
        if (_otherPlayer == null)
            return;
        Clear();
        _otherPlayer.Clear();
    }
    public IEnumerator CreateManagedPlayer(PlayerRegistryMes mes)
    {
        Clear();

        Dictionary<int, PlayerController> playerDic = new();
        foreach (int id in mes.PlayerList)
        {
#if !LOCALDEBUG || !UNITY_EDITOR
            if (id == mes.ClientId)
                continue;
#endif
            if (playerDic.ContainsKey(id))
            {
                Debug.LogWarning($"【托管玩家管理器】玩家{id}重复注册");
                continue;
            }
            var playerViewObj = MonoObjectPool.Instance.GetObject("Player");
            var controller = playerViewObj.GetComponent<PlayerController>();
            playerDic.Add(id, controller);

            _playerObjList.Add(playerViewObj);
            yield return null;
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