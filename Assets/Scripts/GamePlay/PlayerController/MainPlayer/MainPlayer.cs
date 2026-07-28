using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour
{
    public PlayerController playerController;

    [SerializeField]
    [SerializeReference]
    public List<BaseSynSend> stateSynSends = new();
    [SerializeField]
    [SerializeReference]
    public List<BaseFeedbackReceive> feedbackReceives = new();
    public NetReceiver netReceiver { get; private set; } = new();
    private PlayerInput _playerInput;
    private Blackboard _blackboard;

    // Start is called before the first frame update
    void Start()
    {
        netReceiver.StartReceive();
        if (playerController == null)
        {
            Debug.LogError("【主玩家】玩家控制器为空");
            return;
        }
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("【主玩家】玩家输入组件获取失败");
            return;
        }
        _blackboard = playerController.blackboard;
        if (_blackboard == null)
        {
            Debug.LogError("【主玩家】主玩家黑板为空");
            return;
        }

        var abilitySystem = GetComponent<AbilitySystem>();
        if (abilitySystem == null)
            Debug.LogError("【主玩家】AbilitySystem初始化失败");
        else
            abilitySystem.StartAbilitySystem(_blackboard);

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
