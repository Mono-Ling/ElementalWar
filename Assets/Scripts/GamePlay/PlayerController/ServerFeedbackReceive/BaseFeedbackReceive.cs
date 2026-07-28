using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

[Serializable]
public abstract class BaseFeedbackReceive
{
    protected MainPlayer mainPlayer;
    protected Blackboard blackboard;
    public virtual void Init(MainPlayer mainPlayer, Blackboard blackboard)
    {
        this.mainPlayer = mainPlayer;
        this.blackboard = blackboard;
    }
    public virtual void OnRemove() { }
    protected void AddListener<T>(Action<T> action) where T : IMessage
    => mainPlayer?.netReceiver?.AddListener<T>(action);
    protected void RemoveListener<T>(Action<T> action) where T : IMessage
    => mainPlayer?.netReceiver?.RemoveListener<T>(action);
}
