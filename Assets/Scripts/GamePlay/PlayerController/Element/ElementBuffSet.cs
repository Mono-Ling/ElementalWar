using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementBuffSet : MonoBehaviour, IAutoInject<Blackboard>
{
    private HashSet<BaseElementBuff> _buffSet = new();
    private Blackboard _blackboard;
    private ElementListener _elementListener = new();

    private List<BaseElementBuff> _lostList = new();
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
            Debug.LogError("【元素Buff集合】黑板注入为空");
        this._blackboard = inject;
        _blackboard?.SetValue("ElementBuffSet", this);
    }
    void Update()
    {
        foreach (var buff in _buffSet)
            if (buff.TryExit())
                _lostList.Add(buff);

        foreach (var lostBuff in _lostList)
        {
            lostBuff.OnExit();
            _buffSet.Remove(lostBuff);
        }
        _lostList.Clear();

        foreach (var buff in _buffSet)
            buff.OnUpdate();
    }
    void LateUpdate()
    {
        foreach (var buff in _buffSet)
            buff.OnLateUpdate();
    }
    void FixedUpdate()
    {
        foreach (var buff in _buffSet)
            buff.OnFixedUpdate();
    }
    public void AddElementBuff(BaseElementBuff elementBuff)
    {
        if (elementBuff == null || _buffSet.Contains(elementBuff))
            return;
        if (!_blackboard.GetValue<ElementReceiver>("ElementReceiver", out var receiver))
        {
            Debug.LogWarning("【元素Buff集合】元素接收器获取失败");
            return;
        }
        elementBuff.Init(_blackboard, receiver, _elementListener);
        elementBuff.OnEnter();
        _buffSet.Add(elementBuff);

        Debug.Log($"【元素Buff集合】新增buff:{elementBuff.GetType()}");
    }
    public bool Contains(BaseElementBuff elementBuff)
    => _buffSet.Contains(elementBuff);
    public void OnElementTrigger(ElementType elementType)
    => _elementListener?.Trigger(elementType);
}
