using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayerHP : MonoBehaviour, IAutoInject<Blackboard>
{
    public int maxHp = 1000;
    private int _playerHP;
    private Blackboard _blackboard;
    private DynamicTextCreator _dynamicTextCreator;
    void Start()
    {
        _dynamicTextCreator = GetComponent<DynamicTextCreator>();
        if (_dynamicTextCreator == null)
            Debug.LogError("【主玩家生命值组件】动态文本创建器获取失败");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【主玩家生命值组件】黑板注入为空");
            return;
        }
        _blackboard = inject;
        _playerHP = maxHp;
        _blackboard.SetValue("HP", _playerHP);
        _blackboard.SetValue("MaxHP", maxHp);
    }
    public void ReduceHP(int damage, Color color)
    {
        if (damage == 0)
            return;
        _playerHP -= damage;
        _playerHP = Mathf.Max(_playerHP, 0);
        _blackboard.SetValue("HP", _playerHP);
        _dynamicTextCreator?.ShowTextUI(damage.ToString(), color);
    }
    public void ElementDamage(int damage, ElementType element)
    {
        if (ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            ReduceHP(damage, info.color);
        else
            ReduceHP(damage, Color.white);
    }
}
