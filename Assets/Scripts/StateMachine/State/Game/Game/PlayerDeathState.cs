using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerDeathState", menuName = "StateMachine/State/Game/PlayerDeathState")]
public class PlayerDeathState : BaseGameState
{
    public int delayToReset = 5;
    private WaitForSeconds _wait = new(1f);
    private Coroutine _coroutine;
    public override void OnEnter(Blackboard blackboard)
    {
        base.OnEnter(blackboard);
        _coroutine = PublicMono.Instance.StartCoroutine(DelayToPlayerReset());
    }
    public override void OnExit(Blackboard blackboard)
    {
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
}
