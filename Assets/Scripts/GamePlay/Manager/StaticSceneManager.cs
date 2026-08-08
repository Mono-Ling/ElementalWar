using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class StaticSceneManager : SingleMono<StaticSceneManager>
{
    private List<StaticSceneItem> _sceneItemList = new();
    private NetReceiver _netReceiver = new();
    void Start()
    {
        _netReceiver.StartReceive();
    }
    void OnDestroy()
    {
        _netReceiver.StopReceive();
    }
    public void LoadWall(StaticSceneAsset sceneAsset)
    {
        if (sceneAsset == null)
        {
            Debug.LogError("【静态场景管理器】场景资源为空");
            return;
        }
        int count = sceneAsset.sceneInfoList.Count;
        foreach (var info in sceneAsset.sceneInfoList)
            CreateWall(info);

        _netReceiver.AddListener<WallShootHitMessage>(OnWallHit);
    }
    public void Uninstall()
    {
        _netReceiver.RemoveListener<WallShootHitMessage>(OnWallHit);

        foreach (var item in _sceneItemList)
            MonoObjectPool.Instance.PutObject(item.gameObject);
        _sceneItemList.Clear();
        MonoObjectPool.Instance.ClearPool("Wall");
    }
    private void CreateWall(StaticSceneInfo info)
    {
        var obj = MonoObjectPool.Instance.GetObject("Wall");
        var item = obj.GetComponent<StaticSceneItem>();
        if (item == null)
        {
            Debug.LogError("【静态场景管理器】StaticSceneItem获取失败");
            return;
        }
        item.SetInfo(info);
        _sceneItemList.Add(item);
    }
    private void OnWallHit(WallShootHitMessage message)
    {
        if (message == null)
            return;
        if (message.WallId < 0 || message.WallId >= _sceneItemList.Count)
        {
            Debug.LogError("【静态场景管理器】无效索引");
            return;
        }
        (var origin, _) = message.Origin;
        (var dir, _) = message.Dir;
        Space.Ray ray = new(origin, dir);
        _sceneItemList[message.WallId].OnHit(ray, (ElementType)message.ElementType);
    }
}
