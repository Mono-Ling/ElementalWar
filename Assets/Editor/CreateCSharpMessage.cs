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
    const string PROTOC_INCLUDE_PATH = @"D:\Unity\Project\ElementalWar\Protoc\include";
    [MenuItem("Tools/Message/CSharp")]
    private static void CreateCode()
    {
        DirectoryInfo info = Directory.CreateDirectory(PROTOBUF_PATH);
        FileInfo[] files = info.GetFiles();
        foreach (var file in files)
        {
            if (file.Extension != ".proto")
                continue;
            string arg = $"-I={PROTOBUF_PATH} -I={PROTOC_INCLUDE_PATH} --csharp_out={OUTPUT_PATH_1} {file.Name}";
            RunProtoc(arg);
            arg = $"-I={PROTOBUF_PATH} -I={PROTOC_INCLUDE_PATH} --csharp_out={OUTPUT_PATH_2} {file.Name}";
            RunProtoc(arg);
            UnityEngine.Debug.Log($"【生成C#消息代码】{file.Name}");
        }
        AssetDatabase.Refresh();
    }

    private static void RunProtoc(string arg)
    {
        using (Process process = new())
        {
            process.StartInfo.FileName = PROTOC_PATH;
            process.StartInfo.Arguments = arg;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            string stderr = process.StandardError.ReadToEnd();
            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"protoc 执行失败: {arg}\n{stderr}");
            }
        }
    }
}
