using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BaseElementBuff
{
    protected Blackboard blackboard;
    protected ElementReceiver elementReceiver;
    protected ElementAttachment elementAttachment;
    protected ElementBuffSet elementBuffSet;
    protected ElementListener elementListener;

    protected DynamicTextCreator dynamicTextCreator;
    public virtual void Init(Blackboard blackboard, ElementReceiver receiver,
    ElementAttachment attachment, ElementBuffSet buffSet,
    ElementListener listener, DynamicTextCreator dynamicTextCreator)
    {
        this.blackboard = blackboard;
        this.elementReceiver = receiver;
        this.elementAttachment = attachment;
        this.elementBuffSet = buffSet;
        this.elementListener = listener;
        this.dynamicTextCreator = dynamicTextCreator;
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
    protected void AddAttackListener(ElementType elementType, RefAction<float, int> action)
    => elementListener?.AddAttackListener(elementType, action);
    protected void RemoveAttackListener(ElementType elementType, RefAction<float, int> action)
    => elementListener?.RemoveAttackListener(elementType, action);
    public override bool Equals(object obj)
    => obj is BaseElementBuff other && GetType() == other.GetType();
    public override int GetHashCode()
    => GetType().GetHashCode();
}
