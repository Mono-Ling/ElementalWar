using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public abstract class BaseAbility
{
    protected MainPlayer mainPlayer;
    protected PlayerInput playerInput;
    protected Blackboard blackboard;
    public virtual void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        this.mainPlayer = mainPlayer;
        this.playerInput = playerInput;
        this.blackboard = blackboard;
    }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnRemove() { }
    protected void AddInputStartedListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].started += action;
    protected void AddInputPerformedListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].performed += action;
    protected void AddInputCanceledListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].canceled += action;

    protected void RemoveInputStartedListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].started -= action;
    protected void RemoveInputPerformedListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].performed -= action;
    protected void RemoveInputCanceledListener(string name, Action<InputAction.CallbackContext> action)
    => playerInput.actions[name].canceled -= action;
}
