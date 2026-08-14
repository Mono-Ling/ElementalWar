using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElementAttachmentText : BaseUI
{
    public TextMeshProUGUI text;
    public float smoothTime = 0.1f;
    private float _smoothRef;
    private float _targetAlpha;
    protected override void Awake()
    {
        base.Awake();
        if (text == null)
            Debug.LogError("【元素附着文字显示】文本控件为空");
    }
    void OnEnable()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    protected override void Update()
    {
        var alpha = canvasGroup.alpha;
        alpha = Mathf.SmoothDamp(alpha, _targetAlpha, ref _smoothRef, smoothTime);
        canvasGroup.alpha = alpha;
    }
    public void SetElementAttachment(ElementType element, float content)
    {
        if (text == null)
            return;
        if (ElementInfoMap.Instance?.TryGetElementInfo(element, out var info) ?? false)
        {
            text.color = info.color;
            text.text = info.name;

            var num = content / ElementUtility.Content.STRONG;
            num = Mathf.Clamp01(num);
            _targetAlpha = num;
        }
    }
    void OnDisable()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
