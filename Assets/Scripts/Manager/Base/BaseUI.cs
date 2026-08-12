using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseUI : MonoBehaviour
{
    public float showTime = 1f;
    public float hideTime = 2f;
    protected CanvasGroup canvasGroup;
    protected float startAnimationTime;
    protected Action<BaseUI> callback;
    protected bool isShow;
    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            Debug.Log("【UI】CanvasGroup获取失败");
    }
    protected virtual void Update()
    {
        if (isShow)
            ShowAnimation();
        else
            HideAnimation();
    }
    public virtual void Show(Action<BaseUI> action = null, bool isAnimation = true)
    {
        if (!isAnimation)
        {
            action?.Invoke(this);
            return;
        }
        canvasGroup.alpha = 0f;
        startAnimationTime = Time.time;
        callback = action;

        isShow = true;
    }
    public virtual void Hide(Action<BaseUI> action = null, bool isAnimation = true)
    {
        if (!isAnimation)
        {
            action?.Invoke(this);
            return;
        }
        canvasGroup.alpha = 1f;
        startAnimationTime = Time.time;
        callback = action;

        isShow = false;
    }
    protected virtual void ShowAnimation()
    {
        if (Time.time - startAnimationTime > showTime)
        {
            var cb = callback;
            callback = null;
            cb?.Invoke(this);
            canvasGroup.alpha = 1;
            return;
        }
        float t = (Time.time - startAnimationTime) / showTime;
        t = Mathf.Clamp01(t);
        canvasGroup.alpha = t;
    }
    protected virtual void HideAnimation()
    {
        if (Time.time - startAnimationTime > hideTime)
        {
            var cb = callback;
            callback = null;
            cb?.Invoke(this);
            canvasGroup.alpha = 0;
            return;
        }
        float t = (Time.time - startAnimationTime) / hideTime;
        t = Mathf.Clamp01(t);
        canvasGroup.alpha = 1 - t;
    }
}