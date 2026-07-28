using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

[Serializable]
public abstract class BaseSynReceive
{
    protected OtherPlayer otherPlayer;
    protected Dictionary<int, Blackboard> blackboardDic;
    public virtual void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        this.otherPlayer = otherPlayer;
        this.blackboardDic = blackboardDic;
    }
    public virtual void OnRemove() { }
    protected void AddListener<T>(Action<T> action) where T : IMessage
    => otherPlayer.netReceiver?.AddListener<T>(action);
    protected void RemoveListener<T>(Action<T> action) where T : IMessage
    => otherPlayer?.netReceiver?.RemoveListener<T>(action);
}