using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerDeathState", menuName = "StateMachine/State/Game/PlayerDeathState")]
public class PlayerDeathState : State
{
    public int delayToReset = 5;
    private WaitForSeconds _wait = new(1f);
    private Blackboard _blackboard;
    private Coroutine _coroutine;
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【游戏流程-玩家死亡状态】OnEnter黑板为空");
            return;
        }
        _blackboard = blackboard;
        _coroutine = PublicMono.Instance.StartCoroutine(DelayToPlayerReset());
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameEnd);
    }
    public override void OnExit(Blackboard blackboard)
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameEnd);
        if (_coroutine != null)
        {
            PublicMono.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        if (blackboard == null ||
            !blackboard.GetValue<MainPlayer>("MainPlayer", out var mainPlayer))
        {
            Debug.LogError("【游戏流程-玩家死亡状态】主玩家组件获取失败，玩家复活重置失败");
            return;
        }
        mainPlayer?.EndMainPlayer();
        mainPlayer?.InjectBlackboard();
    }
    private IEnumerator DelayToPlayerReset()
    {
        for (int i = 0; i < delayToReset; i++)
            yield return _wait;

        _blackboard?.SetValue("IsDeath", false);
        _coroutine = null;
    }
    private void OnGameEnd(NetPackage netPackage)
    {
        if (netPackage.message is not GameStateMessage message || message.IsStart)
            return;
        _blackboard?.SetValue("IsGameStart", false);
    }
}
