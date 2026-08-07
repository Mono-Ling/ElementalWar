using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualStateMachine : StateMachine, IAutoInject<Blackboard>
{
    private bool _isStart => blackboard != null;
    protected override void Start()
    {
        if (initState == null)
            Debug.LogWarning("手动启动状态机】初始状态为空");
    }
    public void AutoInject(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【手动启动状态机】黑板为空，启动失败");
            return;
        }
        this.blackboard = blackboard;
        currState = GameObject.Instantiate(initState);
        currState.OnEnter(this.blackboard);
    }
    protected override void Update()
    {
        if (!_isStart)
            return;
        base.Update();
    }
    protected override void LateUpdate()
    {
        if (!_isStart)
            return;
        base.LateUpdate();
    }
    protected override void FixedUpdate()
    {
        if (!_isStart)
            return;
        base.FixedUpdate();
    }
}
