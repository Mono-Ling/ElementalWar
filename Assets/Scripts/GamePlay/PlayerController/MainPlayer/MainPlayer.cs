using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer : MonoBehaviour
{
    public PlayerController playerController;
    private Blackboard _blackboard;
    private InitBlackboardArg _initBlackboardArg;
    void Awake()
    {
        _initBlackboardArg = GetComponent<InitBlackboardArg>();
        if (_initBlackboardArg == null)
            Debug.LogError("【主玩家】黑板参数初始化器获取失败");
    }
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
        if (_blackboard == null || _initBlackboardArg == null)
            return;
        _initBlackboardArg.InitArg(_blackboard);

        var components = GetComponents<IAutoInject<Blackboard>>();
        foreach (var item in components)
            item.AutoInject(_blackboard);
    }
}
