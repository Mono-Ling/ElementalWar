using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayerHP : MonoBehaviour, IAutoInject<Blackboard>
{
    public const int DEFAULT_PLAYER_ID = -1;
    public static int AttackFromPlayerId { get; private set; }
    public int maxHp = 1000;
    private int _playerHP;
    private Blackboard _blackboard;
    private DynamicTextCreator _dynamicTextCreator;
    private bool _isDeath;
    void Awake()
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
        _blackboard.SetValue("IsDeath", false);

        SetAttackFrom();
        _isDeath = false;
    }
    public void ReduceHP(int damage, Color color)
    {
        if (damage == 0 || _isDeath)
            return;
        _playerHP -= damage;
        _playerHP = Mathf.Max(_playerHP, 0);
        _blackboard.SetValue("HP", _playerHP);
        _dynamicTextCreator?.ShowTextUI(damage.ToString(), color);

        if (_playerHP == 0)
            OnDeath();
    }
    public void ElementDamage(int damage, ElementType element)
    {
        if (ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            ReduceHP(damage, info.color);
        else
            ReduceHP(damage, Color.white);
    }
    public void SetAttackFrom(int playerId = DEFAULT_PLAYER_ID)
    => AttackFromPlayerId = playerId;
    private void OnDeath()
    {
        Debug.Log("【主玩家生命值组件】玩家死亡");
        _blackboard.SetValue("IsDeath", true);
        _isDeath = true;
        EventBus.Instance.Trigger(EventType.OnPlayerDeath);
    }
}
