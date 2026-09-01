using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

public class LoadingManager : SingleMono<LoadingManager>
{
    private abstract class LoadAsset
    {
        public int Count { get; protected set; }
    }
    private class LoadAsset<T> : LoadAsset, IDisposable where T : Object
    {
        private T _asset;
        public LoadAsset(string path)
        {
            _asset = Resources.Load<T>(path);
            if (_asset == null)
                Debug.LogError($"【资源加载管理器】{path}加载失败");
        }

        public void Dispose()
        {
            if (_asset == null)
                return;
            Resources.UnloadAsset(_asset);
            _asset = null;
        }

        public T Load()
        {

            if (_asset != null)
                ++Count;
            return _asset;
        }
        public bool Uninstall()
        {
            if (_asset == null)
                return false;
            if (Count > 0)
                --Count;
            if (Count == 0)
            {
                Dispose();
                return true;
            }
            return false;
        }
    }
    private Dictionary<string, LoadAsset> _assetDic = new();
    public T Load<T>(string path) where T : Object
    {
        if (_assetDic.TryGetValue(path, out var asset))
            if (asset is LoadAsset<T> findAsset)
                return findAsset.Load();
            else
            {
                Debug.LogError($"【资源加载管理器】{path}资源类型错配");
                return null;
            }
        LoadAsset<T> newAsset = new(path);
        var load = newAsset.Load();
        if (load != null)
            _assetDic.Add(path, newAsset);
        return load;
    }
    public void Uninstall<T>(string path) where T : Object
    {
        if (!_assetDic.TryGetValue(path, out var asset))
            return;
        if (asset is not LoadAsset<T> findAsset)
        {
            Debug.LogError($"【资源加载管理器】{path}资源类型错配");
            return;
        }
        if (findAsset.Uninstall())
            _assetDic.Remove(path);
    }
}