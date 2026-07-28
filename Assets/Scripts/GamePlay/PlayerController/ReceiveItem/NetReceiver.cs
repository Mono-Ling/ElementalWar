using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public class NetReceiver
{
    private Dictionary<Type, ITriggerReceiveEvent> _stateSynEventDic = new();
    public void StartReceive()
    => EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnStateSynMessageReceive);
    public void Clear()
    => _stateSynEventDic.Clear();
    public void StopReceive()
    => EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnStateSynMessageReceive);
    public void AddListener<T>(Action<T> action) where T : IMessage
    {
        if (_stateSynEventDic.TryGetValue(typeof(T), out var baseEvent))
        {
            if (baseEvent is ReceiveEvent<T> synEvent)
                synEvent.action += action;
            else
                Debug.LogError($"【网络监听器】消息类型匹配失败");
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
                Debug.LogError($"【网络监听器】消息类型匹配失败");
        }
        else
            Debug.LogWarning($"【网络监听器】不存在{typeof(T)}消息的监听");
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
    }
}
