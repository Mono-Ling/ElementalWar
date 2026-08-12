using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DynamicText : BaseUI
{
    public Vector3 maxScale;
    public Vector3 minScale;
    public TextMeshProUGUI text;
    void Start()
    {
        if (text == null)
            Debug.LogError("【动态文本UI】文本控件为空");
    }
    public void Init(DynamicTextInfo info)
    {
        if (text == null || !info.IsEffective)
            return;
        text.SetText(info.text);
        text.color = info.color;
    }
    protected override void ShowAnimation()
    {
        if (Time.time - startAnimationTime > showTime)
        {
            var cb = callback;
            callback = null;
            cb?.Invoke(this);
            canvasGroup.alpha = 1;
            text.transform.localScale = maxScale;
            return;
        }
        float t = (Time.time - startAnimationTime) / showTime;
        t = Mathf.Clamp01(t);
        canvasGroup.alpha = t;
        text.transform.localScale = Vector3.Lerp(minScale, maxScale, t);
    }
    protected override void HideAnimation()
    {
        if (Time.time - startAnimationTime > hideTime)
        {
            var cb = callback;
            callback = null;
            cb?.Invoke(this);
            canvasGroup.alpha = 0;
            text.transform.localScale = minScale;
            return;
        }
        float t = (Time.time - startAnimationTime) / hideTime;
        t = Mathf.Clamp01(t);
        canvasGroup.alpha = 1 - t;
        text.transform.localScale = Vector3.Lerp(maxScale, minScale, t);
    }
}
