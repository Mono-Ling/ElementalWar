using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDir : BaseUI
{
    public float radius = 150f;
    public float delayToHide = 1.5f;
    private Coroutine _coroutine;
    public void SetDir(Vector3 dir, Vector3 forward)
    {
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
        var dot = Vector3.Dot(dir, forward);
        var cross = Vector3.Cross(dir, forward);

        dot = Mathf.Clamp(dot, -1f, 1f);
        cross = cross.normalized;
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg * Mathf.Sign(cross.y);
        var rot = Quaternion.AngleAxis(angle, Vector3.forward);
        Vector2 uiDir = rot * Vector2.up * radius;
        transform.localRotation = rot;

        if (transform is RectTransform rectTransform)
            rectTransform.anchoredPosition = uiDir;
    }
    public override void Show(Action<BaseUI> action = null, bool isAnimation = true)
    {
        base.Show(action, isAnimation);
        _coroutine = StartCoroutine(DelayToHide());
    }
    public override void Hide(Action<BaseUI> action = null, bool isAnimation = true)
    {
        base.Hide(action, isAnimation);
        if (_coroutine == null)
            return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }
    private IEnumerator DelayToHide()
    {
        yield return new WaitForSeconds(delayToHide);
        _coroutine = null;
        UIManager.Instance.BufferHideUI(this, UIManager.InitUIPosition);
    }
    public static void ShowHitDir(Vector3 dir, Vector3 forward)
    {
        var dirUI = UIManager.Instance.BufferShowUI<HitDir>();
        if (dirUI == null)
        {
            Debug.LogError("【角色受击显示】受击方向UI获取失败");
            return;
        }
        dirUI.SetDir(dir, forward);
    }
}
