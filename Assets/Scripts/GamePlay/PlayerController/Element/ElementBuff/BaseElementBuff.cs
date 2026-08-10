using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseElementBuff
{
    protected Blackboard blackboard;
    protected ElementReceiver elementReceiver;
    protected ElementListener elementListener;
    public virtual void Init(Blackboard blackboard, ElementReceiver receiver, ElementListener listener)
    {
        this.blackboard = blackboard;
        this.elementReceiver = receiver;
        this.elementListener = listener;
    }
    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnExit() { }
    /// <summary>
    /// 集合中已存在，重复添加buff时调用
    /// </summary>
    public virtual void OnConflict() { }
    /// <summary>
    /// 尝试退出buff
    /// </summary>
    /// <returns>是否退出</returns>
    public abstract bool TryExit();
    protected void AddListener(ElementType elementType, Action<ElementType> action)
    => elementListener?.AddListener(elementType, action);
    protected void RemoveListener(ElementType elementType, Action<ElementType> action)
    => elementListener?.RemoveListener(elementType, action);
    public override bool Equals(object obj)
    => obj is BaseElementBuff other && GetType() == other.GetType();
    public override int GetHashCode()
    => GetType().GetHashCode();
}
