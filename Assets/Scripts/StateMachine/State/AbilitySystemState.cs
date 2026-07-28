using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilitySystemState", menuName = "StateMachine/State/AbilitySystemState")]
public class AbilitySystemState : State
{
    public string abilitySystemArgName = "AbilitySystem";
    [SerializeReference]
    public List<BaseAbility> abilities = new();
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard.GetValue<AbilitySystem>(abilitySystemArgName, out var system))
            system?.SetAbilities(abilities);
        else
            Debug.LogError("【AbilitySystemState】AbilitySystem获取失败");
    }
}
