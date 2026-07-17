using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEmptyState", menuName = "StateMachine/State/Emptystate")]
public class State : ScriptableObject
{
    public List<Edge> edgeList = new();
    protected virtual void OnValidate() => edgeList.Sort((a, b) => b.weight.CompareTo(a.weight));
    public virtual void OnEnter(Blackboard blackboard) { }
    public virtual void OnUpdate(Blackboard blackboard) { }
    public virtual void OnLateUpdate(Blackboard blackboard) { }
    public virtual void OnFixedUpdate(Blackboard blackboard) { }
    public virtual void OnExit(Blackboard blackboard) { }
}
