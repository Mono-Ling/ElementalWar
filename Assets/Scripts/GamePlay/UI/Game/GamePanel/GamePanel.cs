using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamePanel : BaseUI
{
    public TextMeshProUGUI recordText;
    private Blackboard _blackboard;
    protected override void Awake()
    {
        base.Awake();
        if (recordText == null)
            Debug.LogError("【GamePanel】战绩文本控件为空");
    }
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
    public void SetRecord(int killCount, int deathCount)
    {
        if (recordText == null)
            return;
        recordText.text = $"{killCount}\t/\t{deathCount}";
    }
}
