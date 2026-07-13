using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class CreateCSharpMessage
{
    const string PROTOBUF_PATH = @"D:\Unity\Project\ElementalWar\Assets\Editor\Protobuf";
    const string OUTPUT_PATH_1 = @"D:\Unity\Project\ElementalWar\Assets\Scripts\Net\Message";
    const string OUTPUT_PATH_2 = @"D:\Unity\Project\ElementalWar\Server\Message";
    const string PROTOC_PATH = @"D:\Unity\Project\ElementalWar\Protoc\protoc.exe";
    [MenuItem("Tools/Message/CSharp")]
    private static void CreateCode()
    {
        DirectoryInfo info = Directory.CreateDirectory(PROTOBUF_PATH);
        FileInfo[] files = info.GetFiles();
        foreach (var file in files)
        {
            if (file.Extension != ".proto")
                continue;
            Process process = new();
            process.StartInfo.FileName = PROTOC_PATH;
            string arg = $"-I={PROTOBUF_PATH} --csharp_out={OUTPUT_PATH_1} {file.Name}";
            process.StartInfo.Arguments = arg;
            process.Start();

            process = new();
            process.StartInfo.FileName = PROTOC_PATH;
            arg = $"-I={PROTOBUF_PATH} --csharp_out={OUTPUT_PATH_2} {file.Name}";
            process.StartInfo.Arguments = arg;
            process.Start();
            UnityEngine.Debug.Log("【生成C#消息代码】");
            AssetDatabase.Refresh();
        }
    }
}
