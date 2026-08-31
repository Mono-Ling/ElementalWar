using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public abstract class BaseGameState : State
{
    protected Blackboard _blackboard;
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【游戏流程-游戏中状态】OnEnter黑板为空");
            return;
        }
        _blackboard = blackboard;
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameEnd);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnKillPlayer);
    }
    public override void OnExit(Blackboard blackboard)
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameEnd);
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnKillPlayer);
    }
    private void OnKillPlayer(NetPackage netPackage)
    {
        if (netPackage.message is not DeathMessage message)
            return;
        if (_blackboard.GetValue("KillCount", out int count))
            _blackboard.SetValue("KillCount", ++count);
        KillView.ShowKillView();
        UpdateRecord();
    }
    private void OnGameEnd(NetPackage netPackage)
    {
        if (netPackage.message is not GameStateMessage message || message.IsStart)
            return;
        _blackboard?.SetValue("IsGameStart", false);
    }
    protected void UpdateRecord()
    {
        if (!_blackboard.GetValue("KillCount", out int kill) ||
            !_blackboard.GetValue("DeathCount", out int death))
            return;
        if (UIManager.Instance.TryGetCurrentPanel<GamePanel>(out var panel))
            panel?.SetRecord(kill, death);
    }
}
