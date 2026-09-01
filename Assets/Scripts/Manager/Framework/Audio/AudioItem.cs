using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioItem : MonoBehaviour
{
    private AudioSource _audioSource;
    private Action _endAction;
    private Coroutine _coroutine;
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogError("【AudioItem】AudioSource获取失败");
    }
    void OnEnable()
    => transform.position = Vector3.zero;
    void OnDisable()
    {
        if (_coroutine == null)
            return;
        StopCoroutine(_coroutine);
        _coroutine = null;
        OnAudioEnd();
    }
    public void Play(AudioClip audioClip, bool isLoop = false, Action endAction = null)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("【AudioItem】音频文件为空，播放失败");
            MonoObjectPool.Instance.PutObject(gameObject);
            return;
        }
        _audioSource.clip = audioClip;
        _audioSource.loop = isLoop;
        _endAction = endAction;
        _audioSource.Play();
        if (isLoop)
            return;
        _coroutine = StartCoroutine(WaitToAudioEnd());
    }
    public void SetWorld(bool isWorld = false)
    {
        _audioSource.spatialize = isWorld;
        _audioSource.spatialBlend = isWorld ? 1f : 0f;
    }
    private IEnumerator WaitToAudioEnd()
    {
        yield return new WaitWhile(() => _audioSource.isPlaying);
        _coroutine = null;
        OnAudioEnd();
    }
    private void OnAudioEnd()
    {
        _endAction?.Invoke();
        _endAction = null;
        MonoObjectPool.Instance.PutObject(gameObject);
    }
}
