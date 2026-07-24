using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public int playerId { get; private set; }
    public PlayerController playerController;
    [SerializeReference]
    public List<BaseSynReceive> stateSynReceiveList = new();
    private Dictionary<Type, ITriggerStateSynReceiveEvent> _stateSynEventDic = new();
    // Start is called before the first frame update
    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError($"【网络玩家{playerId}】玩家控制组件为空");
            return;
        }
        var blackboard = playerController.blackboard;
        if (blackboard == null)
        {
            Debug.LogError($"【网络玩家{playerId}】玩家黑板获取失败");
            return;
        }

        foreach (var synReceive in stateSynReceiveList)
            synReceive.Init(this, blackboard);
    }
    void OnDestroy()
    {
        foreach (var synReceive in stateSynReceiveList)
            synReceive.OnRemove();
    }
    public void InitOtherPlayer(int id, PlayerController playerController)
    {
        this.playerId = id;
        this.playerController = playerController;
    }
    public void AddListener<T>(Action<T> action) where T : IMessage
    {
        if (_stateSynEventDic.TryGetValue(typeof(T), out var baseEvent))
        {
            if (baseEvent is StateSynReceiveEvent<T> synEvent)
                synEvent.action += action;
            else
                Debug.LogError($"【网络玩家{playerId}】消息类型匹配失败");
        }
        else
        {
            StateSynReceiveEvent<T> synEvent = new();
            synEvent.action += action;
            _stateSynEventDic.Add(typeof(T), synEvent);
        }
    }
    public void RemoveListener<T>(Action<T> action) where T : IMessage
    {
        if (_stateSynEventDic.TryGetValue(typeof(T), out var baseEvent))
        {
            if (baseEvent is StateSynReceiveEvent<T> synEvent)
                synEvent.action -= action;
            else
                Debug.LogError($"【网络玩家{playerId}】消息类型匹配失败");
        }
        else
            Debug.LogWarning($"【网络玩家{playerId}】不存在{typeof(T)}消息的监听");
    }
    public void OnStateSynMessageReceive(IMessage message)
    {
        if (_stateSynEventDic.TryGetValue(message.GetType(), out var baseEvent))
        {
            baseEvent.Trigger(message);
        }
        else
            Debug.LogWarning($"【网络玩家{playerId}】不存在{message.GetType()}消息的监听");
    }
}
public interface ITriggerStateSynReceiveEvent
{
    void Trigger(IMessage message);
}
public class StateSynReceiveEvent<T> : ITriggerStateSynReceiveEvent where T : IMessage
{
    public event Action<T> action;

    public void Trigger(IMessage message)
    {
        if (message is not T statesynMessage)
            Debug.LogError("【状态同步接收事件】类型转换失败");
        else
            action?.Invoke(statesynMessage);
    }
}