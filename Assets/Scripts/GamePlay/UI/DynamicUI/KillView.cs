using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillView : BaseUI
{
    private static KillView _currAction;
    public float delayToHide = 1f;
    private Coroutine _coroutine;
    private WaitForSeconds _wait;
    protected override void Awake()
    {
        base.Awake();
        _wait = new(delayToHide);
    }
    public override void Show(Action<BaseUI> action = null, bool isAnimation = true)
    {
        base.Show(action, isAnimation);
        _coroutine = StartCoroutine(DelayToHide());
        _currAction = this;
    }
    public override void Hide(Action<BaseUI> action = null, bool isAnimation = true)
    {
        base.Hide(action, isAnimation);
        _currAction = null;
        if (_coroutine == null)
            return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }
    private void Reset()
    {
        if (_coroutine == null)
            return;
        StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(DelayToHide());
    }
    private IEnumerator DelayToHide()
    {
        yield return _wait;
        _coroutine = null;
        UIManager.Instance.BufferHideUI(this, UIManager.InitUIPosition);
    }
    public static void ShowKillView()
    {
        if (_currAction != null)
        {
            _currAction.Reset();
            return;
        }
        var view = UIManager.Instance.BufferShowUI<KillView>(UIManager.InitUIPosition);
        if (view == null)
        {
            Debug.LogError("【角色击杀显示】角色击杀UI显示失败");
        }
    }
}
