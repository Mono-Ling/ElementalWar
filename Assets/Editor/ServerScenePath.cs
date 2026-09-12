using System.IO;

/// <summary>
/// 服务端场景烘焙产物的导出目录：工程根下的 Server/Scene/。
/// </summary>
public static class ServerScenePath
{
    /// <summary>工程根目录下的 Server/Scene/</summary>
    public static readonly string SCENE_DIR = ProjectPath.Resolve("Server/Scene");

    /// <summary>拼接导出文件的完整路径，并确保目录存在</summary>
    public static string Resolve(string fileName)
    {
        Directory.CreateDirectory(SCENE_DIR);
        return Path.Combine(SCENE_DIR, fileName);
    }
}
