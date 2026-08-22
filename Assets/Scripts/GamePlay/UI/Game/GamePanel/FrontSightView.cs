using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontSightView : MonoBehaviour, IAutoInject<Blackboard>
{
    public GameObject normal;
    public GameObject aim;
    private BlackboardArg<bool> _isAimArg;
    void Awake()
    {
        if (normal == null)
            Debug.LogError("【准星显示】常规准星为空");
        if (aim == null)
            Debug.LogError("【准星显示】瞄准准星为空");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【准星显示】黑板设置为空");
            return;
        }
        if (!inject.GetBlackboardArg("IsAim", out _isAimArg))
            Debug.LogError("【准星显示】瞄准黑板参数获取失败");
        if (_isAimArg != null)
            _isAimArg.OnValueChange += OnAimChange;

        OnAimChange(_isAimArg?.value ?? default);
    }
    void OnDestroy()
    {
        if (_isAimArg != null)
            _isAimArg.OnValueChange -= OnAimChange;
    }
    private void OnAimChange(bool isAim)
    {
        if (isAim)
        {
            aim?.SetActive(true);
            normal?.SetActive(false);
        }
        else
        {
            aim?.SetActive(false);
            normal?.SetActive(true);
        }
    }
}
