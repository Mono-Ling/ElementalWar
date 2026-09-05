using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingleMono<AudioManager>
{
    private const string FOLDER_PATH = "Audio";
    private const string ITEM_PATH = "AudioItem";
    public void PlaySound(string path, GameObject worldObj = null, Action endAction = null)
    {
        var clip = LoadingManager.Instance.Load<AudioClip>($"{FOLDER_PATH}/{path}");
        if (clip == null)
        {
            Debug.LogError($"【音频管理器】{path}音频加载失败");
            return;
        }
        var item = CreateAudioItem();
        if (item == null)
        {
            LoadingManager.Instance.Uninstall<AudioClip>($"{FOLDER_PATH}/{path}");
            return;
        }
        endAction += () => OnAudioEnd(path);
        if (worldObj != null)
            item.transform.position = worldObj.transform.position;
        else
            item.transform.SetParent(transform, false);
        // 空间音频相关设置必须在Play前
        item.SetWorld(worldObj != null);
        item.Play(clip, false, endAction);
    }
    private AudioItem CreateAudioItem()
    {
        var obj = MonoObjectPool.Instance.GetObject($"{FOLDER_PATH}/{ITEM_PATH}");
        if (obj == null)
        {
            Debug.LogError("【音频管理器】AudioItem创建失败");
            return null;
        }
        var item = obj.GetComponent<AudioItem>();
        if (item == null)
        {
            MonoObjectPool.Instance.PutObject(obj);
            Debug.LogError("【音频管理器】AudioItem组件获取失败");
            return null;
        }
        return item;
    }
    private void OnAudioEnd(string path)
    => LoadingManager.Instance.Uninstall<AudioClip>($"{FOLDER_PATH}/{path}");
}