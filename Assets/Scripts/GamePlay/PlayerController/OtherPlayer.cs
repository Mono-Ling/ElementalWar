using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    [SerializeReference]
    public List<BaseSynReceive> stateSynReceiveList = new();
    private Dictionary<int, PlayerController> _otherPlayerDic = new();
    private Dictionary<Type, ITriggerReceiveEvent> _stateSynEventDic = new();
    void Start()
    {
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnStateSynMessageReceive);
    }
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
        _stateSynEventDic.Clear();
    }
    void OnDestroy()
    {
        Clear();
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnStateSynMessageReceive);
    }

    public void AddListener<T>(Action<T> action) where T : IMessage
    {
        if (_stateSynEventDic.TryGetValue(typeof(T), out var baseEvent))
        {
            if (baseEvent is ReceiveEvent<T> synEvent)
                synEvent.action += action;
            else
                Debug.LogError($"【网络玩家】消息类型匹配失败");
        }
        else
        {
            ReceiveEvent<T> synEvent = new();
            synEvent.action += action;
            _stateSynEventDic.Add(typeof(T), synEvent);
        }
    }
    public void RemoveListener<T>(Action<T> action) where T : IMessage
    {
        if (_stateSynEventDic.TryGetValue(typeof(T), out var baseEvent))
        {
            if (baseEvent is ReceiveEvent<T> synEvent)
                synEvent.action -= action;
            else
                Debug.LogError($"【网络玩家】消息类型匹配失败");
        }
        else
            Debug.LogWarning($"【网络玩家】不存在{typeof(T)}消息的监听");
    }
    private void OnStateSynMessageReceive(NetPackage package)
    {
        if (package.sendType != SendType.Udp || package.message == null)
            return;
        IMessage message = package.message;
        if (_stateSynEventDic.TryGetValue(message.GetType(), out var baseEvent))
        {
            baseEvent.Trigger(message);
        }
        else
            Debug.LogWarning($"【网络玩家】不存在{message.GetType()}消息的监听");
    }
}