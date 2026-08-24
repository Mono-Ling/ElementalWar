using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    [SerializeReference]
    public List<BaseSynReceive> stateSynReceiveList = new();
    public NetReceiver netReceiver { get; private set; } = new();
    private Dictionary<int, PlayerController> _otherPlayerDic = new();
    private Dictionary<Type, ITriggerReceiveEvent> _stateSynEventDic = new();
    void Start()
    => netReceiver.StartReceive();
    public void InitOtherPlayer(Dictionary<int, PlayerController> playerDic)
    {
        if (playerDic == null)
        {
            Debug.LogError("【网络玩家】玩家字典为空");
            return;
        }
        Clear();
        Dictionary<int, Blackboard> blackboardDic = new();
        foreach (var item in playerDic)
        {
            int id = item.Key;
            var controller = item.Value;
            if (controller == null)
            {
                Debug.LogError($"【网络玩家{id}】玩家控制器为空");
                continue;
            }
            if (blackboardDic.ContainsKey(id))
            {
                Debug.LogWarning($"【网络玩家{id}】重复注册");
                continue;
            }
            var blackboard = controller.blackboard;
            if (blackboard == null)
            {
                Debug.LogError($"【网络玩家{id}】黑板获取失败");
                return;
            }

            blackboard.SetValue("PlayerId", id);

            blackboardDic.Add(id, blackboard);
        }

        foreach (var synReceive in stateSynReceiveList)
            synReceive.Init(this, blackboardDic);
    }
    public void Clear()
    {
        foreach (var synReceive in stateSynReceiveList)
            synReceive.OnRemove();

        _otherPlayerDic.Clear();
        netReceiver.Clear();
    }
    void OnDestroy()
    {
        Clear();
        netReceiver.StopReceive();
    }
}