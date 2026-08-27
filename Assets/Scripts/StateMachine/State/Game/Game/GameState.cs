using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameState", menuName = "StateMachine/State/Game/GameState")]
public class GameState : State
{
    private Blackboard _blackboard;
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【游戏流程-游戏状态】OnEnter黑板为空");
            return;
        }
        _blackboard = blackboard;
        _blackboard.SetValue("IsDeath", false);

        if (!_blackboard.GetValue<MainPlayer>("MainPlayer", out var mainPlayer))
        {
            Debug.LogError("【游戏流程-游戏状态】主玩家获取失败");
            _blackboard.SetValue("IsGameStart", false);
            return;
        }
        if (!UIManager.Instance.TryGetCurrentPanel<GamePanel>(out _))
            UIManager.Instance.ShowPanel<GamePanel>();
        mainPlayer?.StartMainPlayer();

        EventBus.Instance.AddListener(EventType.OnPlayerDeath, OnPlayerDeath);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameEnd);
    }
    public override void OnExit(Blackboard blackboard)
    {
        EventBus.Instance.RemoveListener(EventType.OnPlayerDeath, OnPlayerDeath);
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameEnd);
    }
    private void OnPlayerDeath()
    {
        _blackboard?.SetValue("IsDeath", true);
    }
    private void OnGameEnd(NetPackage netPackage)
    {
        if (netPackage.message is not GameStateMessage message || message.IsStart)
            return;
        _blackboard?.SetValue("IsGameStart", false);
    }
}
