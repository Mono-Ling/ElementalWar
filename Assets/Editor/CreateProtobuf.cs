using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateProtobuf
{
    private const string TEMPLATE_PATH = "Assets/Editor/Protobuf/Template/Template.txt";
    //private const string CREAT_PATH = "Assets/Editor/Protobuf/NewProtobuf.proto";
    [MenuItem("Assets/Create/Protobuf")]
    private static void Create()
    {
        // 获取当前选中文件夹
        Object selected = Selection.activeObject;
        string selectPath = AssetDatabase.GetAssetPath(selected);
        if (!AssetDatabase.IsValidFolder(selectPath))
        {
            Debug.LogError("请先选中一个文件夹再创建Protobuf");
            return;
        }

        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(TEMPLATE_PATH);
        if (textAsset == null)
        {
            Debug.LogError("【模板加载失败】Protobuf模板加载失败");
            return;
        }

        string fileName = "NewProtobuf.proto";
        string relativePath = Path.Combine(selectPath, fileName);
        string fullPath = Path.Combine(Application.dataPath, relativePath.Substring("Assets/".Length));

        File.WriteAllText(fullPath, textAsset.text);
        AssetDatabase.ImportAsset(relativePath);
    }
}
