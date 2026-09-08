using System.Collections;
using System.Collections.Generic;
using Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BeginPanel : BaseUI
{
    public Button butMatch;
    public Button butQuit;
    public TextMeshProUGUI matchText;
    public string matchStr = "匹配";
    public string quitMatchStr = "取消匹配";
    private Blackboard _blackboard;
    private BlackboardArg<bool> _isMatchArg;

    private PlayerMatchMessage _matchMes = new();
    protected override void Awake()
    {
        base.Awake();
        if (butMatch == null)
        {
            Debug.LogError("【BeginPanel】匹配按钮为空");
            return;
        }
        butMatch.onClick.AddListener(OnMatchClick);
        if (butQuit == null)
        {
            Debug.LogError("【BeginPanel】退出按钮为空");
            return;
        }
        butQuit.onClick.AddListener(OnQuit);

        if (matchText == null)
            Debug.LogError("【BeginPanel】匹配文本为空");
    }
    public void SetBlackboard(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【BeginPanel】黑板设置为空");
            return;
        }
        _blackboard = blackboard;
        if (!_blackboard.GetBlackboardArg("IsMatch", out _isMatchArg))
        {
            Debug.LogError("【BeginPanel】匹配黑板参数获取失败");
            return;
        }
        _isMatchArg.OnValueChange += OnMatchChanged;
        OnMatchChanged(_isMatchArg.value);
    }
    void OnDisable()
    {
        if (butMatch != null)
            butMatch.onClick.RemoveListener(OnMatchClick);
        if (butQuit != null)
            butQuit.onClick.RemoveListener(OnQuit);
        if (_isMatchArg != null)
            _isMatchArg.OnValueChange -= OnMatchChanged;
    }
    void OnDestroy()
    => OnDisable();
    private void OnMatchClick()
    {
        if (_blackboard == null || !_blackboard.GetValue("IsMatch", out bool isMatch))
        {
            Debug.LogError("【BeginPanel】匹配黑板参数获取失败");
            return;
        }
        _blackboard.SetValue("IsMatch", !isMatch);

        _matchMes.IsMatch = !isMatch;
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(_matchMes));
    }
    private void OnQuit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    private void OnMatchChanged(bool isMatch)
    {
        if (matchText == null)
            return;
        matchText.text = isMatch ? quitMatchStr : matchStr;
    }
}
