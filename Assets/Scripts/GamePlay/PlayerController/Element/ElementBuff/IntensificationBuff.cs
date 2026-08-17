using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 激化buff
/// </summary>
public class IntensificationBuff : BaseElementBuff
{
    [Header("元素量")]
    [Range(0, ElementUtility.Content.STRONG)]
    public float elementContent = ElementUtility.Content.WEAK;
    [Header("超激化")]
    public string veryIntensificationName = "超激化";
    public Color veryIntensificationColor;
    public int veryIntensificationDamage = 60;
    [Header("蔓激化")]
    public string vineIntensificationName = "蔓激化";
    public Color vineIntensificationColor;
    public int vineIntensificationDamage = 70;
    public float duration;
    public float delayListen;
    private float _startTime;
    private Action _listenAction;
    public override bool TryExit()
    => Time.time - _startTime >= duration;
    public override void OnEnter()
    {
        _startTime = Time.time;
        _listenAction += DelayToListen;
    }
    public override void OnUpdate()
    {
        if (Time.time - _startTime >= delayListen)
            _listenAction?.Invoke();
    }
    public override void OnExit()
    {
        RemoveListener(ElementType.Thunder, OnThunderTrigger);
        RemoveListener(ElementType.Grass, OnGrassTrigger);

        RemoveListener(ElementType.Fire, OnRemove);
        RemoveListener(ElementType.Water, OnRemove);

        _listenAction = null;
        Debug.Log("【激化Buff】退出激化状态");
    }
    private void DelayToListen()
    {
        AddListener(ElementType.Thunder, OnThunderTrigger);
        AddListener(ElementType.Grass, OnGrassTrigger);

        AddListener(ElementType.Fire, OnRemove);
        AddListener(ElementType.Water, OnRemove);

        _listenAction -= DelayToListen;
        Debug.Log("【激化Buff】注册监听");
    }
    private void OnThunderTrigger(ElementType elementType)
    {
        // _startTime = Time.time;
        // Debug.Log("【原激化Buff】超激化");
        var content = elementContent;
        var damage = vineIntensificationDamage;
        TriggerAttackListener(ElementType.Thunder, ref content, ref damage);
        if (damage == 0 || content < 0.001)
            return;
        playerHP?.ReduceHP(damage, veryIntensificationColor);
        dynamicTextCreator?.ShowTextUI(veryIntensificationName, veryIntensificationColor);
    }
    private void OnGrassTrigger(ElementType elementType)
    {
        // _startTime = Time.time;
        // Debug.Log("【原激化Buff】蔓激化");
        var content = elementContent;
        var damage = vineIntensificationDamage;
        TriggerAttackListener(ElementType.Grass, ref content, ref damage);
        if (damage == 0 || content < 0.001)
            return;
        playerHP?.ReduceHP(damage, vineIntensificationColor);
        dynamicTextCreator?.ShowTextUI(vineIntensificationName, vineIntensificationColor);
    }
    private void OnRemove(ElementType elementType)
    => elementBuffSet?.TryRemoveElementBuff(this);
}