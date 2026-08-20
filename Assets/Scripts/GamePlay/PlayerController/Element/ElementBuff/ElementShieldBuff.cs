using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementShieldBuff : BaseElementBuff
{
    public float delay = 20f;
    public float maxContent = ElementUtility.Content.STRONG * 2;
    private float _currContent;
    private float _startTime;
    private ElementType _element;
    public override bool TryExit()
    => _currContent <= 0 || Time.time - _startTime >= delay;
    public void InitElementShield(ElementType element)
    {
        if (element == ElementType.None)
            return;
        if (_element != ElementType.None)
            RemoveAttackListener(_element, OnElementAttack);
        _element = element;
        AddAttackListener(_element, OnElementAttack);

        _currContent = maxContent;
        _startTime = Time.time;

        Debug.Log($"【元素护盾】添加{element}元素护盾");
    }
    public override void OnExit()
    {
        RemoveAttackListener(_element, OnElementAttack);
        Debug.Log("【元素护盾】消失");
    }
    private void OnElementAttack(ref float content, ref int damage)
    {
        float deltaContent = Mathf.Min(_currContent, content);
        float num = 1 - Mathf.Clamp01(deltaContent / content);
        damage = Mathf.CeilToInt(num * damage);
        content -= deltaContent;
        _currContent -= deltaContent;
    }
}