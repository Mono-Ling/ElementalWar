using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Message;
using UnityEngine;

public class DynamicSceneItemMgr : SingleMono<DynamicSceneItemMgr>
{
    private NetReceiver _netReceiver = new();
    private Dictionary<int, BaseDynamicSceneItem> _remoteDynamicItemDic = new();
    private HashSet<BaseDynamicSceneItem> _localDynamicItemSet = new();
    void Start()
    {
        _netReceiver.StartReceive();
        _netReceiver.AddListener<DynamicItemStateMes>(OnDynamicStateMes);
    }
    void OnDestroy()
    {
        _netReceiver.RemoveListener<DynamicItemStateMes>(OnDynamicStateMes);
        _netReceiver.StopReceive();
    }
    private void OnDynamicStateMes(DynamicItemStateMes mes)
    {
        if (mes == null)
            return;
        switch (mes.StateType)
        {
            case DynamicItemStateMes.Types.DynamicItemStateType.Create:
                OnRemoteCreate(mes);
                break;
            case DynamicItemStateMes.Types.DynamicItemStateType.Destroy:
                OnRemoteDestroy(mes);
                break;
            default:
                Debug.LogWarning("【动态物体管理器】无效远程动态物体同步指令");
                break;
        }
    }
    private void OnRemoteCreate(DynamicItemStateMes mes)
    {
        if (_remoteDynamicItemDic.ContainsKey(mes.DynamicItemId))
        {
            Debug.LogWarning($"【动态物体管理器】动态物体{mes.DynamicItemId}重复注册");
            return;
        }
        var path = GetItemPath(mes.ItemType);
        GameObject obj = null;
        if (path != null)
            obj = MonoObjectPool.Instance.GetObject(path);
        if (obj == null)
        {
            Debug.LogError($"【动态物体管理器】远程动态物体{mes.ItemType}创建失败");
            return;
        }
        BaseDynamicSceneItem item = obj.GetComponent<BaseDynamicSceneItem>();
        if (item == null)
        {
            Debug.LogError("【动态物体管理器】BaseDynamicSceneItem获取失败");
            MonoObjectPool.Instance.PutObject(obj);
            return;
        }
        item.OnRemoteCreate(_netReceiver, mes);
        _remoteDynamicItemDic.Add(mes.DynamicItemId, item);
        Debug.Log($"【动态物体管理器】远程动态物体注册{mes.DynamicItemId}");
    }
    private void OnRemoteDestroy(DynamicItemStateMes mes)
    {
        if (_remoteDynamicItemDic.TryGetValue(mes.DynamicItemId, out var item))
        {
            item.OnRemoteDestroy(mes);
            _remoteDynamicItemDic.Remove(mes.DynamicItemId);
            MonoObjectPool.Instance.PutObject(item.gameObject);
        }
        else
            Debug.LogWarning($"【动态物体管理器】远程动态物体{mes.DynamicItemId}不存在");
    }
    public BaseDynamicSceneItem CreateLocalDynamicSceneItem(DynamicSceneItemType type)
    {
        var path = GetItemPath(type);
        GameObject obj = null;
        if (path != null)
            obj = MonoObjectPool.Instance.GetObject(path);
        if (obj == null)
        {
            Debug.LogError("【动态物体管理器】本地动态物体创建失败");
            return null;
        }
        BaseDynamicSceneItem item = obj.GetComponent<BaseDynamicSceneItem>();
        if (item == null)
        {
            Debug.LogError("【动态物体管理器】BaseDynamicSceneItem获取失败");
            MonoObjectPool.Instance.PutObject(obj);
            return null;
        }
        item.LocalCreate(type);
        _localDynamicItemSet.Add(item);
        return item;
    }
    public void DestroyLocalDynamicSceneItem(BaseDynamicSceneItem dynamicSceneItem)
    {
        if (dynamicSceneItem == null)
            return;
        if (_localDynamicItemSet.Contains(dynamicSceneItem))
        {
            dynamicSceneItem.LocalDestroy();
            _localDynamicItemSet.Remove(dynamicSceneItem);
            MonoObjectPool.Instance.PutObject(dynamicSceneItem.gameObject);
        }
        else
            Debug.LogWarning($"【动态物体管理器】本地动态物体{dynamicSceneItem}不存在", dynamicSceneItem.gameObject);
    }
    public void ClearLocal()
    {
        var localList = _localDynamicItemSet.ToList();
        foreach (var item in localList)
            DestroyLocalDynamicSceneItem(item);
    }
    private string GetItemPath(DynamicSceneItemType type) => type switch
    {
        DynamicSceneItemType.Grenade => "Grenade",
        DynamicSceneItemType.GrenadeExp => "GrenadeExp",
        _ => null,
    };
}