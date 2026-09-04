using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateSceneBKAsset
{
    private const string PATH = "Assets/SO/SceneBKAsset/NewSceneBKAsset.asset";
    [MenuItem("GameObject/Create/SceneBKAsset")]
    private static void Create()
    {
        var asset = ScriptableObject.CreateInstance<SceneBKAsset>();
        var objs = Selection.gameObjects;
        asset.Set(objs);
        AssetDatabase.CreateAsset(asset, PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
