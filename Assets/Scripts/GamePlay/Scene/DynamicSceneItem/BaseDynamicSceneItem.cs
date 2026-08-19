using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;
using Google.Protobuf;
using System;

public abstract class BaseDynamicSceneItem : MonoBehaviour
{
    private static int _clientGlobalDynamicSceneItemId = 0;
    protected static int GetDynamicSceneItemId => _clientGlobalDynamicSceneItemId++;
    protected bool isRemote;
    protected NetReceiver netReceiver;
    protected DynamicSceneItemType itemType;
    /// <summary>
    /// 当前物体类型，ItemNone 表示尚未完成创建初始化
    /// </summary>
    public DynamicSceneItemType ItemType => itemType;
    protected int dynamicSceneItemId;
    public virtual void LocalCreate(DynamicSceneItemType itemType)
    {
        if (itemType == DynamicSceneItemType.ItemNone)
        {
            Debug.LogWarning("【动态场景物体】无效的动态场景物体类型，创建同步失败");
            return;
        }
        int id = GetDynamicSceneItemId;
        DynamicItemStateMes mes = new()
        {
            DynamicItemId = id,
            ItemType = itemType,
            StateType = DynamicItemStateMes.Types.DynamicItemStateType.Create
        };
        SendTo(mes, true);

        isRemote = false;
        this.itemType = itemType;
        dynamicSceneItemId = id;
    }
    public virtual void LocalCreate<T>(DynamicSceneItemType itemType, T arg)
    => LocalCreate(itemType);
    public virtual void LocalDestroy()
    {
        if (itemType == DynamicSceneItemType.ItemNone)
        {
            Debug.LogWarning("【动态场景物体】无效的动态场景物体类型，销毁同步失败");
            return;
        }
        DynamicItemStateMes mes = new()
        {
            DynamicItemId = dynamicSceneItemId,
            ItemType = itemType,
            StateType = DynamicItemStateMes.Types.DynamicItemStateType.Destroy,
        };
        SendTo(mes);
    }
    public virtual void OnRemoteCreate(NetReceiver netReceiver, DynamicItemStateMes mes)
    {
        this.netReceiver = netReceiver;
        isRemote = true;
        dynamicSceneItemId = mes.DynamicItemId;
    }
    public virtual void OnRemoteDestroy(DynamicItemStateMes mes) { }
    protected void SendTo(IMessage message, bool isResponse = false)
    {
        if (message == null)
            return;
        UdpHeader udpHeader = new() { IsResponse = isResponse };
        NetPackage package = new(udpHeader, message);
        EventBus.Instance.Trigger(EventType.SendTo, package);
    }
    protected void AddListener<T>(Action<T> action) where T : IMessage
    => netReceiver?.AddListener<T>(action);
    protected void RemoveListener<T>(Action<T> action) where T : IMessage
    => netReceiver?.RemoveListener<T>(action);
}
