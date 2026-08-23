using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSubStateMachine", menuName = "StateMachine/State/SubStateMachine")]
public class SubStateMachine : State
{
    public string lockStateArgName = "IsLockState";
    public bool isWriteLockArg = true;
    public State anyState;
    public State initState;
    public State endState;
    public bool isDebug;
    private State _initState;
    private State _currState;
    public override void OnEnter(Blackboard blackboard)
    {
        if (initState == null)
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
        _initState = initState;
        _currState = GameObject.Instantiate(initState);
        _currState?.OnEnter(blackboard);
    }
    public override void OnUpdate(Blackboard blackboard)
    {
        if (!TryChangeState(anyState, blackboard))
            if (!TryChangeState(_currState, blackboard))
                _currState?.OnUpdate(blackboard);
    }
    public override void OnLateUpdate(Blackboard blackboard) => _currState?.OnLateUpdate(blackboard);
    public override void OnFixedUpdate(Blackboard blackboard) => _currState?.OnFixedUpdate(blackboard);
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
            if (edge.targetState.Equals(_currState))
                continue;
            if (edge.condition.IsCompleted(blackboard))
            {
                if (state == anyState)
                {
                    _currState.OnExit(blackboard);
                    if (isDebug) Debug.Log("【子状态机】任意状态切换");
                }
                else
                    state.OnExit(blackboard);

                _currState = edge.targetState;

                if (_currState == endState)
                {
                    if (isDebug)
                        Debug.Log("【子状态机】退出子状态机");

                    if (isWriteLockArg)
                        blackboard.SetValue<bool>(lockStateArgName, false);

                    // 回退初始状态，为下一次进入子状态机做准备
                    initState = _initState;
                    return true;
                }
                _currState = GameObject.Instantiate(_currState);
                _currState.OnEnter(blackboard);
                if (isDebug)
                    Debug.Log($"【子状态机】转换至：{_currState}");
                return true;
            }
        }
        return false;
    }
}
