using System.IO;
using UnityEngine;

/// <summary>
/// Unity 工程路径解析：一律以工程根（Assets 的上一级）为基准推导，
/// 编辑器工具中不再写死盘符绝对路径，工程可任意迁移。
/// </summary>
public static class ProjectPath
{
    /// <summary>工程根目录</summary>
    public static readonly string ROOT = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    /// <summary>把工程内相对路径（如 "Server/Message"）解析为绝对路径</summary>
    public static string Resolve(string relativePath) => Path.GetFullPath(Path.Combine(ROOT, relativePath));

    /// <summary>解析工程内相对目录，并确保目录存在</summary>
    public static string ResolveDir(string relativePath) => Directory.CreateDirectory(Resolve(relativePath)).FullName;
}
