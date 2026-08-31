using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameState", menuName = "StateMachine/State/Game/GameState")]
public class GameState : BaseGameState
{
    public override void OnEnter(Blackboard blackboard)
    {
        base.OnEnter(blackboard);
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

        UpdateRecord();
    }
    public override void OnExit(Blackboard blackboard)
    {
        EventBus.Instance.RemoveListener(EventType.OnPlayerDeath, OnPlayerDeath);
    }
    private void OnPlayerDeath()
    {
        _blackboard?.SetValue("IsDeath", true);
        if (_blackboard.GetValue("DeathCount", out int count))
            _blackboard.SetValue("DeathCount", ++count);
        UpdateRecord();
    }
}
