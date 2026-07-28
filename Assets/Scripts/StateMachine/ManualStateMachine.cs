using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualStateMachine : StateMachine
{
    private bool _isStart => blackboard != null;
    protected override void Start() { }
    public void InitStateMachine(Blackboard blackboard)
    {
        if (blackboard == null)
            Debug.LogError("【手动启动状态机】黑板为空，启动失败");
        this.blackboard = blackboard;
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
