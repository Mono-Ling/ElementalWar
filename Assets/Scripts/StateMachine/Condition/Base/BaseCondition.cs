using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCondition : ScriptableObject
{
    public abstract bool IsCompleted(Blackboard blackboard);
}
