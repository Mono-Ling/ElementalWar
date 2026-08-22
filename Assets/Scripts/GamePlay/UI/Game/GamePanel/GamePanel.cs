using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePanel : BaseUI
{
    private Blackboard _blackboard;
    public void SetBlackboard(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【GamePanel】黑板设置为空");
            return;
        }
        _blackboard = blackboard;
        var compontents = GetComponents<IAutoInject<Blackboard>>();
        foreach (var item in compontents)
            item?.AutoInject(_blackboard);
    }
}
