using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEdge", menuName = "StateMachine/Edge")]
public class Edge : ScriptableObject
{
    public BaseCondition condition;
    public State targetState;
    public int weight;
    void OnValidate()
    {
        if (condition == null)
            Debug.LogError("【状态机边】转换条件为空");
        if (targetState == null)
            Debug.LogError("【状态机边】目标状态为空");
    }
}
