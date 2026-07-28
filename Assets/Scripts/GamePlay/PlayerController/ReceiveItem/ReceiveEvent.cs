using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public interface ITriggerReceiveEvent
{
    void Trigger(IMessage message);
}
public class ReceiveEvent<T> : ITriggerReceiveEvent where T : IMessage
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
