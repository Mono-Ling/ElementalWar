using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour, IGameStart, IGameEnd
{
    public State anyState;
    public State initState;
    public bool isDebug;
    protected Blackboard blackboard;
    protected State currState;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        blackboard = GetComponent<Blackboard>();
        if (blackboard == null)
        {
            Debug.LogError("【状态机】黑板获取失败");
            return;
        }
        if (initState == null)
        {
            Debug.LogWarning("【状态机】初始状态为空");
            return;
        }
        OnGameStart();
    }
    public void OnGameStart()
    {
        if (blackboard == null)
            return;
        currState = GameObject.Instantiate(initState);
        currState.OnEnter(blackboard);
    }
    public void OnGameEnd()
    => currState?.OnExit(blackboard);

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!TryChangeState(anyState))
            if (!TryChangeState(currState))
                currState?.OnUpdate(blackboard);
    }
    protected virtual void LateUpdate() => currState?.OnLateUpdate(blackboard);
    protected virtual void FixedUpdate() => currState?.OnFixedUpdate(blackboard);
    protected virtual bool TryChangeState(State state)
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
            if (edge.targetState.Equals(currState))
                continue;
            if (edge.condition.IsCompleted(blackboard))
            {
                if (state == anyState)
                    currState.OnExit(blackboard);
                else
                    state.OnExit(blackboard);

                currState = GameObject.Instantiate(edge.targetState);
                currState.OnEnter(blackboard);
                if (isDebug)
                    Debug.Log($"【状态机】转换至：{currState}");
                return true;
            }
        }
        return false;
    }
}
