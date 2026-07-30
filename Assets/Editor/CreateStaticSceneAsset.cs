using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateStaticSceneAsset : Editor
{
    private const string PATH = "Assets/SO/SceneAsset/NewSceneAsset.asset";
    [MenuItem("GameObject/Create/SceneAsset")]
    private static void CreateAsset()
    {
        var asset = ScriptableObject.CreateInstance<StaticSceneAsset>();
        var objs = Selection.gameObjects;
        foreach (var obj in objs)
            GetItemInfo(obj, asset);
        AssetDatabase.CreateAsset(asset, PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private static void GetItemInfo(GameObject obj, StaticSceneAsset asset)
    {
        var item = obj.GetComponent<StaticSceneItem>();
        if (item == null)
        {
            Debug.LogError("【静态地图资产创建】StaticSceneItem获取失败", obj);
            return;
        }
        var info = item.info;
        if (info == null)
        {
            Debug.LogError("【静态地图资产创建】StaticSceneInfo获取失败", obj);
            return;
        }
        asset.sceneInfoList.Add(info);
    }
}
