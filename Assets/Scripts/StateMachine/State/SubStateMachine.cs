using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSubStateMachine", menuName = "StateMachine/State/SubStateMachine")]
public class SubStateMachine : State
{
    public string lockStateArgName = "IsLockState";
    public State anyState;
    public State currState;
    public State endState;
    public bool isDebug;
    public override void OnEnter(Blackboard blackboard)
    {
        if (currState == null)
        {
            Debug.LogError("【子状态机】初始状态为空");
            return;
        }
        if (endState == null)
        {
            Debug.LogError("【子状态机】退出状态为空");
            return;
        }
        blackboard.SetValue<bool>(lockStateArgName, true);
        currState?.OnEnter(blackboard);
    }
    public override void OnUpdate(Blackboard blackboard)
    {
        if (!TryChangeState(anyState, blackboard))
            TryChangeState(currState, blackboard);
        currState?.OnUpdate(blackboard);
    }
    public override void OnLateUpdate(Blackboard blackboard) => currState?.OnLateUpdate(blackboard);
    public override void OnFixedUpdate(Blackboard blackboard) => currState?.OnFixedUpdate(blackboard);
    private bool TryChangeState(State state, Blackboard blackboard)
    {
        if (state == null)
            return false;
        foreach (Edge edge in state.edgeList)
        {
            if (edge.condition == null || edge.targetState == null)
            {
                Debug.LogWarning("【状态机】无效出边");
                continue;
            }
            if (edge.condition.IsCompleted(blackboard))
            {
                if (state == anyState)
                    currState.OnExit(blackboard);
                else
                    state.OnExit(blackboard);

                currState = edge.targetState;

                if (currState == endState)
                {
                    if (isDebug)
                        Debug.Log("【子状态机】退出子状态机");
                    blackboard.SetValue<bool>(lockStateArgName, false);
                    return true;
                }
                currState.OnEnter(blackboard);
                if (isDebug)
                    Debug.Log($"【子状态机】转换至：{currState}");
                return true;
            }
        }
        return false;
    }
}
