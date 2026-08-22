using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FrontSightAbility : BaseAbility, IEquatable<FrontSightAbility>
{
    public enum FrontSightType
    {
        None,
        Normal,
        Aim,
    }
    public FrontSightType frontSightType = FrontSightType.Normal;
    private BaseUI _uI;
    public override void InitAbility(AbilitySystem abilitySystem, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(abilitySystem, playerInput, blackboard);

        switch (frontSightType)
        {
            case FrontSightType.Normal:
                _uI = UIManager.Instance.BufferShowUI<NormalFrontSight>();
                InitUI(_uI);
                break;
            case FrontSightType.Aim:
                _uI = UIManager.Instance.BufferShowUI<AimFrontSight>();
                InitUI(_uI);
                break;
        }
    }
    public override void OnRemove()
    {
        if (_uI != null)
            UIManager.Instance.BufferHideUI(_uI);
    }
    private void InitUI(BaseUI uI)
    {
        if (uI == null || uI.transform is not RectTransform rect)
            return;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = new(1, 1, 1);
    }

    public bool Equals(FrontSightAbility other)
    => frontSightType == other.frontSightType;
}
