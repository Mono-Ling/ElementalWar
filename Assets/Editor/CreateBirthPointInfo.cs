using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateBirthPointInfo : Editor
{
    private const string NAME = "BirthPoint";
    private const string PATH = @"D:\Unity\Project\ElementalWar\Server\Scene\";
    [MenuItem("Tools/Create/ServerBirthPointInfo")]
    private static void Create()
    {
        var objs = Selection.gameObjects;
        BirthPointInfo asset = new();
        foreach (var obj in objs)
        {
            if (obj == null)
                continue;
            asset.positions.Add(obj.transform.position);
        }
        string json = JsonUtility.ToJson(asset);
        File.WriteAllText($"{PATH}{NAME}.json", json);
    }
}
