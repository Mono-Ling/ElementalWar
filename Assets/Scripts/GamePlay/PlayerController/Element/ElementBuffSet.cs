using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ElementReceiver))]
public class ElementBuffSet : MonoBehaviour, IAutoInject<Blackboard>
{
    public ElementBuffMap elementBuffMap;
    private HashSet<BaseElementBuff> _buffSet = new();
    private Blackboard _blackboard;
    private ElementReceiver _elementReceiver;
    private ElementListener _elementListener = new();

    private DynamicTextCreator _dynamicTextCreator;

    private List<BaseElementBuff> _lostList = new();
    void Awake()
    {
        if (elementBuffMap == null)
            Debug.LogError("【元素Buff集合】元素Buff注册表为空");

        _elementReceiver = GetComponent<ElementReceiver>();
        if (_elementReceiver == null)
            Debug.LogError("【元素Buff集合】元素接收器组件获取失败");

        _dynamicTextCreator = GetComponent<DynamicTextCreator>();
        if (_dynamicTextCreator == null)
            Debug.LogError("【元素Buff集合】动态字体UI创建组件获取失败");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
            Debug.LogError("【元素Buff集合】黑板注入为空");
        this._blackboard = inject;
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
    public void AddElementBuff<T>() where T : BaseElementBuff
    {
        if (elementBuffMap == null)
            return;
        if (elementBuffMap.TryGetElementBuff<T>(out var buff))
            AddElementBuff(buff);
        else
            Debug.LogWarning($"【元素Buff集合】{typeof(T)}未注册");
    }
    public bool TryRemoveElementBuff<T>() where T : BaseElementBuff
    {
        if (elementBuffMap == null)
            return false;
        if (elementBuffMap.TryGetElementBuff<T>(out var buff))
            return TryRemoveElementBuff(buff);
        else
            Debug.LogWarning($"【元素Buff集合】{typeof(T)}未注册");
        return false;
    }
    private void AddElementBuff(BaseElementBuff elementBuff)
    {
        if (elementBuff == null)
            return;
        if (_buffSet.TryGetValue(elementBuff, out var buff))
        {
            buff.OnConflict();
            return;
        }
        if (_elementReceiver == null)
        {
            Debug.LogWarning("【元素Buff集合】元素接收器获取失败");
            return;
        }
        if (!_blackboard.GetValue<ElementAttachment>("ElementAttachment", out var attachment))
        {
            Debug.LogWarning("【元素Buff集合】元素附着组件获取失败");
            return;
        }
        elementBuff.Init(
            _blackboard, _elementReceiver,
            attachment, this,
            _elementListener, _dynamicTextCreator);
        elementBuff.OnEnter();
        _buffSet.Add(elementBuff);

        Debug.Log($"【元素Buff集合】新增buff:{elementBuff.GetType()}");
    }
    public bool TryRemoveElementBuff(BaseElementBuff elementBuff)
    {
        if (elementBuff == null)
            return false;
        if (_buffSet.TryGetValue(elementBuff, out var buff))
        {
            buff.OnExit();
            _buffSet.Remove(buff);
            return true;
        }
        else
            return false;
    }
    public bool Contains<T>() where T : BaseElementBuff
    {
        if (elementBuffMap == null)
            return false;
        if (elementBuffMap.TryGetElementBuff<T>(out var buff))
            return Contains(buff);
        else
            Debug.LogWarning($"【元素Buff集合】{typeof(T)}未注册");
        return false;
    }
    public bool Contains(BaseElementBuff elementBuff)
    => _buffSet.Contains(elementBuff);
    public void OnElementTrigger(ElementType elementType)
    => _elementListener?.Trigger(elementType);
    public void OnElementAttackTrigger(ElementType elementType, ref float content, ref int damage)
    => _elementListener?.TriggerAttackListener(elementType, ref content, ref damage);
}
