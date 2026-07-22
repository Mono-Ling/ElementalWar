using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSubStateMachine", menuName = "StateMachine/State/SubStateMachine")]
public class SubStateMachine : State
{
    public string lockStateArgName = "IsLockState";
    public bool isWriteLockArg = true;
    public State anyState;
    public State currState;
    public State endState;
    public bool isDebug;
    private State _initState;
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
        if (isWriteLockArg) blackboard.SetValue<bool>(lockStateArgName, true);
        currState?.OnEnter(blackboard);
        _initState = currState;
    }
    public override void OnUpdate(Blackboard blackboard)
    {
        if (!TryChangeState(anyState, blackboard))
            if (!TryChangeState(currState, blackboard))
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
                {
                    currState.OnExit(blackboard);
                    if (isDebug) Debug.Log("【子状态机】任意状态切换");
                }
                else
                    state.OnExit(blackboard);

                currState = edge.targetState;

                if (currState == endState)
                {
                    if (isDebug)
                        Debug.Log("【子状态机】退出子状态机");

                    if (isWriteLockArg)
                        blackboard.SetValue<bool>(lockStateArgName, false);

                    // 回退初始状态，为下一次进入子状态机做准备
                    currState = _initState;
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
