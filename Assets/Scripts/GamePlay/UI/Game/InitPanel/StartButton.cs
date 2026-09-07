using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour, InitPanel.IShowInitPanel
{
    public Button startButton;
    public CanvasGroup butCanvasGroup;
    public float showTime = 1f;
    private bool _isShow;
    private float _startTime;
    void Start()
    {
        if (startButton == null)
        {
            Debug.LogError("【初始化面板-开始按钮】按钮控件为空");
            return;
        }
        startButton.onClick.AddListener(OnStart);
        if (butCanvasGroup == null)
            Debug.LogError("【初始化面板-开始按钮】CanvasGroup为空");
    }
    void Update()
    {
        if (!_isShow || butCanvasGroup == null || 1 - butCanvasGroup.alpha < 0.001)
            return;
        float t = (Time.time - _startTime) / showTime;
        t = Mathf.Clamp01(t);
        butCanvasGroup.alpha = t;
    }
    void OnDestroy()
    {
        if (startButton == null)
            return;
        startButton.onClick.RemoveAllListeners();
    }
    private void OnStart()
    => NetManager.Instance.StartClient();
    public void OnInitPanelShow()
    {
        _isShow = true;
        _startTime = Time.time;
    }
}
