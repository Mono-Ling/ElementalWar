using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateServerStaticSceneAsset : Editor
{
    private const string PATH = @"D:\Unity\Project\ElementalWar\Server\Scene\";
    [MenuItem("Tools/Create/ServerStaticSceneAsset")]
    private static void Create()
    {
        var selObj = Selection.activeObject;
        if (selObj is not StaticSceneAsset asset)
            return;
        string json = JsonUtility.ToJson(asset);
        File.WriteAllText($"{PATH}{asset.name}.json", json);
    }
}
