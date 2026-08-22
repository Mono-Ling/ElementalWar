using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIStarter : MonoBehaviour, IAutoInject<Blackboard>
{
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【Game UI 启动器】黑板注入为空，初始化失败");
            return;
        }
        var panel = UIManager.Instance.ShowPanel<GamePanel>();
        UIManager.InitUIPosition(panel);
        panel?.SetBlackboard(inject);
    }
}
