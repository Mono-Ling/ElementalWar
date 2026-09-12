using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;

public class CreateCSharpMessage
{
    // 工程内相对路径，由 ProjectPath 以工程根推导为绝对路径（不再写死盘符）
    static readonly string PROTOBUF_PATH = ProjectPath.ResolveDir("Assets/Editor/Protobuf");
    static readonly string OUTPUT_PATH_1 = ProjectPath.ResolveDir("Assets/Scripts/Net/Message");
    static readonly string OUTPUT_PATH_2 = ProjectPath.ResolveDir("Server/Message");
    static readonly string PROTOC_PATH = ProjectPath.Resolve("Protoc/protoc.exe");
    static readonly string PROTOC_INCLUDE_PATH = ProjectPath.Resolve("Protoc/include");

    [MenuItem("Tools/Message/CSharp")]
    private static void CreateCode()
    {
        if (!File.Exists(PROTOC_PATH))
        {
            UnityEngine.Debug.LogError($"【生成C#消息代码】未找到 protoc：{PROTOC_PATH}");
            return;
        }

        DirectoryInfo info = new(PROTOBUF_PATH);
        FileInfo[] files = info.GetFiles();
        foreach (var file in files)
        {
            if (file.Extension != ".proto")
                continue;
            RunProtoc(file.Name, OUTPUT_PATH_1);
            RunProtoc(file.Name, OUTPUT_PATH_2);
            UnityEngine.Debug.Log($"【生成C#消息代码】{file.Name}");
        }
        AssetDatabase.Refresh();
    }

    private static void RunProtoc(string protoFileName, string outputPath)
    {
        using (Process process = new())
        {
            process.StartInfo.FileName = PROTOC_PATH;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            // 手工加引号以兼容含空格的工程路径：Unity 引用的 unity-4.8-api 不提供 ProcessStartInfo.ArgumentList
            process.StartInfo.Arguments =
                $"-I=\"{PROTOBUF_PATH}\" -I=\"{PROTOC_INCLUDE_PATH}\" --csharp_out=\"{outputPath}\" \"{protoFileName}\"";
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
                UnityEngine.Debug.LogError($"【生成C#消息代码】protoc 执行失败｜输出:{outputPath}｜{protoFileName}\n{process.StandardError.ReadToEnd()}");
        }
    }
}
