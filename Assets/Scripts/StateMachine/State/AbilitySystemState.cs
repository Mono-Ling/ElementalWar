using System;
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
    public override bool Equals(object other)
    {
        if (other is not AbilitySystemState ability)
            return false;
        if (ability.abilities == null || ability.abilities.Count != abilities.Count)
            return false;
        for (int i = 0; i < abilities.Count; i++)
            if (ability.abilities[i] != abilities[i])
                return false;
        return true;
    }
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (var item in abilities)
            hash.Add(item);
        return hash.ToHashCode();
    }
}
