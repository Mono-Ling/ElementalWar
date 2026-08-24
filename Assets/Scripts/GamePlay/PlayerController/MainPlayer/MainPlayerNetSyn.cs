using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerNetSyn : MonoBehaviour, IAutoInject<Blackboard>, IGameStart
{

    [SerializeReference]
    public List<BaseSynSend> stateSynSends = new();
    [SerializeReference]
    public List<BaseFeedbackReceive> feedbackReceives = new();
    public NetReceiver netReceiver { get; private set; } = new();
    private Blackboard _blackboard;
    private bool _isStart;
    void Start()
    => netReceiver.StartReceive();
    public void AutoInject(Blackboard blackboard)
    {
        _blackboard = blackboard;

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
    }
    void LateUpdate()
    {
        if (!_isStart)
            return;
        foreach (var send in stateSynSends)
            send.OnLateUpdate();
    }
    void FixedUpdate()
    {
        if (!_isStart)
            return;
        foreach (var send in stateSynSends)
            send.OnFixedUpdate();
    }
    void OnDestroy()
    {
        if (_isStart)
            foreach (var send in stateSynSends)
                send.OnRemove();

        foreach (var receiver in feedbackReceives)
            receiver.OnRemove();

        netReceiver.StopReceive();
    }
}
