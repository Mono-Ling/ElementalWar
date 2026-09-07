using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitPanel : BaseUI
{
    public interface IShowInitPanel
    {
        void OnInitPanelShow();
    }
    public override void Show(Action<BaseUI> action = null, bool isAnimation = true)
    {
        base.Show(action, isAnimation);
        callback += OnShow;
    }
    private void OnShow(BaseUI uI)
    {
        var cs = GetComponents<IShowInitPanel>();
        foreach (var c in cs)
            c.OnInitPanelShow();
    }
}
