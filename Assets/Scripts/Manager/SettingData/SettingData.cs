using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public abstract class SettingData<T> : Single<T> where T : class
{
    public abstract void Load();
    public abstract void Save();
    protected void EnsureFolder(params string[] paths)
    {
        if (paths.Length == 0)
            return;
        string fullPath = Path.Combine(paths);
        Directory.CreateDirectory(fullPath);
    }
    protected bool ExistFile(params string[] paths)
    {
        if (paths.Length == 0)
            return false;
        var fullPath = Path.Combine(paths);
        return File.Exists(fullPath);
    }
}
