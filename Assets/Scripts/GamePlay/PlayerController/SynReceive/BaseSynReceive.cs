using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

[Serializable]
public abstract class BaseSynReceive
{
    protected OtherPlayer otherPlayer;
    protected Blackboard blackboard;
    public virtual void Init(OtherPlayer otherPlayer, Blackboard blackboard)
    {
        this.otherPlayer = otherPlayer;
        this.blackboard = blackboard;
    }
    public virtual void OnRemove() { }
    protected void AddListener<T>(Action<T> action) where T : IMessage
    => otherPlayer.AddListener<T>(action);
    protected void RemoveListener<T>(Action<T> action) where T : IMessage
    => otherPlayer.RemoveListener<T>(action);
}
