using System;
using System.IO;

namespace Server.Scene
{
    /// <summary>
    /// 场景烘焙资源的运行时路径解析。
    /// 资源与可执行文件同级（Scene/），由 Server.csproj 在构建时复制
    /// </summary>
    public static class ScenePath
    {
        public const string SCENE_DIR = "Scene";

        /// <summary>场景资源目录：程序集所在目录下的 Scene/</summary>
        public static string Root { get; } = Path.Combine(AppContext.BaseDirectory, SCENE_DIR);

        /// <summary>拼接场景资源文件的完整路径</summary>
        public static string Resolve(string fileName) => Path.Combine(Root, fileName);
    }
}
