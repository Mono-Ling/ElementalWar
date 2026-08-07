using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    public PlayerController playerController;
    private Blackboard _blackboard;
    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("【主玩家】玩家控制器为空");
            return;
        }
        _blackboard = playerController.blackboard;
        if (_blackboard == null)
        {
            Debug.LogError("【主玩家】主玩家黑板为空");
            return;
        }

        InjectBlackboard();
    }
    private void InjectBlackboard()
    {
        if (_blackboard == null)
            return;
        var components = GetComponents<IAutoInject<Blackboard>>();
        foreach (var item in components)
            item.AutoInject(_blackboard);
    }
}
