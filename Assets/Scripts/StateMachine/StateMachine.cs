using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public State anyState;
    public State currState;
    public bool isDebug;
    private Blackboard _blackboard;
    // Start is called before the first frame update
    void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【状态机】黑板获取失败");
            return;
        }
        if (currState == null)
        {
            Debug.LogWarning("【状态机】初始状态为空");
            return;
        }
        currState.OnEnter(_blackboard);
    }

    // Update is called once per frame
    void Update()
    {
        if (!TryChangeState(anyState))
            TryChangeState(currState);
        currState?.OnUpdate(_blackboard);
    }
    void LateUpdate() => currState?.OnLateUpdate(_blackboard);
    void FixedUpdate() => currState?.OnFixedUpdate(_blackboard);
    private bool TryChangeState(State state)
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
            if (edge.condition.IsCompleted(_blackboard))
            {
                if (state == anyState)
                    currState.OnExit(_blackboard);
                else
                    state.OnExit(_blackboard);

                currState = edge.targetState;
                currState.OnEnter(_blackboard);
                if (isDebug)
                    Debug.Log($"【状态机】转换至：{currState}");
                return true;
            }
        }
        return false;
    }
}
