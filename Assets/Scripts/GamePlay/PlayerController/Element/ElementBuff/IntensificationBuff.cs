using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 激化buff
/// </summary>
public class IntensificationBuff : BaseElementBuff
{
    private float _duration;
    private float _delayListen;
    private float _startTime;
    private bool _isListen;
    public IntensificationBuff(float duration, float delay)
    {
        _duration = Mathf.Abs(duration);
        _delayListen = Mathf.Abs(delay);
        _delayListen = Mathf.Min(_duration, _delayListen);
    }
    public override bool TryExit()
    => Time.time - _startTime >= _duration;
    public override void OnEnter()
    => _startTime = Time.time;
    public override void OnUpdate()
    {
        if (!_isListen && Time.time - _startTime >= _delayListen)
        {
            AddListener(ElementType.Thunder, OnThunderTrigger);
            AddListener(ElementType.Grass, OnGrassTrigger);

            AddListener(ElementType.Fire, OnRemove);
            AddListener(ElementType.Water, OnRemove);
            _isListen = true;
        }
    }
    public override void OnExit()
    {
        RemoveListener(ElementType.Thunder, OnThunderTrigger);
        RemoveListener(ElementType.Grass, OnGrassTrigger);

        RemoveListener(ElementType.Fire, OnRemove);
        RemoveListener(ElementType.Water, OnRemove);
        Debug.Log("【激化Buff】退出激化状态");
    }
    private void OnThunderTrigger(ElementType elementType)
    {
        _startTime = Time.time;
        Debug.Log("【原激化Buff】超激化");
    }
    private void OnGrassTrigger(ElementType elementType)
    {
        _startTime = Time.time;
        Debug.Log("【原激化Buff】蔓激化");
    }
    private void OnRemove(ElementType elementType)
    => elementBuffSet?.TryRemoveElementBuff(this);
}