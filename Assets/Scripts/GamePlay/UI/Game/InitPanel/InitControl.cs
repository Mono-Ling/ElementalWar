using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitControl : MonoBehaviour, InitPanel.IShowInitPanel
{
    public Button startButton;
    public Button netSettingButton;
    public CanvasGroup butCanvasGroup;
    public float showTime = 1f;
    private bool _isShow;
    private float _startTime;
    void Start()
    {
        if (startButton == null || netSettingButton == null)
        {
            Debug.LogError("【初始化面板-控件】按钮控件为空");
            return;
        }
        startButton.onClick.AddListener(OnStart);
        netSettingButton.onClick.AddListener(OnNetSetting);
        if (butCanvasGroup == null)
            Debug.LogError("【初始化面板-控件】CanvasGroup为空");
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
        startButton?.onClick.RemoveAllListeners();
        netSettingButton?.onClick.RemoveAllListeners();
    }
    private void OnStart()
    => NetManager.Instance.StartClient();
    private void OnNetSetting()
    => UIManager.Instance.ShowPanel<NetSettingPanel>();
    public void OnInitPanelShow()
    {
        _isShow = true;
        _startTime = Time.time;
    }
}
