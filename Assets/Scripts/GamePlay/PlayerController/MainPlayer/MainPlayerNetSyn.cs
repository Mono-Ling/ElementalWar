using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayerNetSyn : MonoBehaviour, IAutoInject<Blackboard>
{

    [SerializeReference]
    public List<BaseSynSend> stateSynSends = new();
    [SerializeReference]
    public List<BaseFeedbackReceive> feedbackReceives = new();
    public NetReceiver netReceiver { get; private set; } = new();
    private Blackboard _blackboard;
    void Start()
    => netReceiver.StartReceive();
    public void AutoInject(Blackboard blackboard)
    {
        _blackboard = blackboard;
        foreach (var send in stateSynSends)
        {
            send.Init(_blackboard);
            send.Init(this);
        }
        foreach (var receiver in feedbackReceives)
            receiver.Init(this, _blackboard);
    }
    void Update()
    {
        foreach (var send in stateSynSends)
            send.OnUpdate();
    }
    void LateUpdate()
    {
        foreach (var send in stateSynSends)
            send.OnLateUpdate();
    }
    void FixedUpdate()
    {
        foreach (var send in stateSynSends)
            send.OnFixedUpdate();
    }
    void OnDestroy()
    {
        foreach (var send in stateSynSends)
            send.OnRemove();

        foreach (var receiver in feedbackReceives)
            receiver.OnRemove();

        netReceiver.StopReceive();
    }
}
