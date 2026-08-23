using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ElementTypeSelectAbility : BaseAbility
{
    private static ElementType _elementType = ElementType.Fire;
    public bool isDebug;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);
        AddInputStartedListener("ElementType", OnElementTypeStarted);

        if (blackboard.GetValue<ElementType>("AttackElementType", out var element))
            _elementType = element;
        // blackboard.SetValue("AttackElementType", _elementType);
    }
    private void OnElementTypeStarted(InputAction.CallbackContext context)
    {
        var control = context.control;
        if (int.TryParse(control.name, out int num))
            ElementTypeChange(num);
        else
            Debug.LogWarning("【ElementTypeSelectAbility】按键数字解析失败");
    }
    public override void OnRemove()
    => RemoveInputStartedListener("ElementType", OnElementTypeStarted);
    private void ElementTypeChange(int index)
    {
        index--;
        index = 1 << index;
        if (!ElementUtility.TryToElementType(index, out var element))
            return;
        _elementType = element;
        blackboard.SetValue("AttackElementType", _elementType);
        if (isDebug)
            Debug.Log($"【ElementTypeSelectAbility】{_elementType}");
    }
    public override bool Equals(object obj)
    => typeof(ElementTypeSelectAbility) == obj.GetType();
    public override int GetHashCode()
    => typeof(ElementTypeSelectAbility).GetHashCode();
}