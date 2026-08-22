using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour, IAutoInject<Blackboard>
{
    public Image hpBar;
    public Image smoothedBar;
    public TextMeshProUGUI text;
    public float smoothTime = 0.1f;
    private float _smoothRef;
    private float _smoothedProgress;
    private float _targetProgress;
    private Blackboard _blackboard;
    private BlackboardArg<int> _hpArg;
    private BlackboardArg<int> _maxHpArg;
    void Awake()
    {
        if (hpBar == null)
            Debug.LogError("【HP Bar】血条图片控件为空");
        if (smoothedBar == null)
            Debug.LogError("【HP Bar】缓动血条图片控件为空");
        if (text == null)
            Debug.LogError("【HP Bar】文本控件为空");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【HP Bar】黑板设置为空");
            return;
        }
        _blackboard = inject;
        if (!_blackboard.GetBlackboardArg("HP", out _hpArg))
            Debug.LogError("【HP Bar】血量黑板参数获取失败");
        if (!_blackboard.GetBlackboardArg("MaxHP", out _maxHpArg))
            Debug.LogError("【HP Bar】最大血量黑板参数获取失败");

        if (_hpArg != null)
            _hpArg.OnValueChange += OnHPChanged;
        if (_maxHpArg != null)
            _maxHpArg.OnValueChange += OnMaxHPChanged;
        UpdateView(_hpArg?.value ?? default, _maxHpArg?.value ?? default);
    }
    void OnDestroy()
    {
        if (_hpArg != null)
            _hpArg.OnValueChange -= OnHPChanged;
        if (_maxHpArg != null)
            _maxHpArg.OnValueChange -= OnMaxHPChanged;
    }
    void Update()
    {
        _smoothedProgress = Mathf.SmoothDamp(
            _smoothedProgress,
            _targetProgress,
            ref _smoothRef, smoothTime);
        if (smoothedBar != null)
            smoothedBar.fillAmount = _smoothedProgress;
    }
    private void OnHPChanged(int value)
        => UpdateView(value, _maxHpArg?.value ?? default);
    private void OnMaxHPChanged(int value)
    => UpdateView(_hpArg?.value ?? default, value);
    private void UpdateView(int hp, int maxHP)
    {
        if (maxHP <= 0)
        {
            _targetProgress = 0;
            return;
        }
        hp = Mathf.Max(hp, 0);
        maxHP = Mathf.Max(maxHP, 0);
        _targetProgress = Mathf.Clamp01((float)hp / maxHP);
        if (hpBar != null)
            hpBar.fillAmount = _targetProgress;
        if (text != null)
            text.text = $"{hp}/{maxHP}";
    }
}
