using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerNetSyn : MonoBehaviour, IAutoInject<Blackboard>, IGameStart, IGameEnd
{

    [SerializeReference]
    public List<BaseSynSend> stateSynSends = new();
    [SerializeReference]
    public List<BaseFeedbackReceive> feedbackReceives = new();
    public NetReceiver netReceiver { get; private set; } = new();
    private Blackboard _blackboard;
    private bool _isStart;
    public void AutoInject(Blackboard blackboard)
    {
        _blackboard = blackboard;

        netReceiver.StartReceive();

        foreach (var receiver in feedbackReceives)
            receiver.Init(this, _blackboard);
    }
    public void OnGameStart()
    {
        _isStart = true;
        foreach (var send in stateSynSends)
        {
            send.Init(_blackboard);
            send.Init(this);
        }
    }
    void Update()
    {
        if (!_isStart)
            return;
        foreach (var send in stateSynSends)
            send.OnUpdate();
        foreach (var receive in feedbackReceives)
            receive.OnUpdate();
    }
    void LateUpdate()
    {
        if (!_isStart)
            return;
        foreach (var send in stateSynSends)
            send.OnLateUpdate();
        foreach (var receive in feedbackReceives)
            receive.OnLateUpdate();
    }
    void FixedUpdate()
    {
        if (!_isStart)
            return;
        foreach (var send in stateSynSends)
            send.OnFixedUpdate();
        foreach (var receive in feedbackReceives)
            receive.OnFixedUpdate();
    }
    public void OnGameEnd()
    {
        if (_isStart)
            foreach (var send in stateSynSends)
                send.OnRemove();

        foreach (var receiver in feedbackReceives)
            receiver.OnRemove();

        netReceiver?.StopReceive();

        _isStart = false;
    }
    void OnDestroy()
    => OnGameEnd();
}
