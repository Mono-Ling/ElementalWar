using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIStarter : MonoBehaviour, IAutoInject<Blackboard>, IGameStart
{
    private Blackboard _blackboard;
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【Game UI 启动器】黑板注入为空，初始化失败");
            return;
        }
        _blackboard = inject;

    }
    public void OnGameStart()
    {
        if (UIManager.Instance.TryGetCurrentPanel<GamePanel>(out var panel))
            panel?.SetBlackboard(_blackboard);
        else
            Debug.LogError("【游戏UI启动器】GamePanel获取失败");
    }
}
