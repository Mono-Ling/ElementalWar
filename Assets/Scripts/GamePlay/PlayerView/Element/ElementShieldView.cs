using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementShieldView : MonoBehaviour
{
    public Vector3 offset = new(0, 0.85f, 0);
    private Blackboard _blackboard;
    private BlackboardArg<ElementType> _elementShieldTypeArg;
    private ElementShield _elementShield;
    void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【角色元素护盾显示】黑板获取失败");
            return;
        }
        if (_blackboard.GetBlackboardArg("ElementShieldType", out _elementShieldTypeArg))
            _elementShieldTypeArg.OnValueChange += OnElementShieldTypeChange;
        else
            Debug.LogError("【角色元素护盾显示】护盾类型黑板参数获取失败");
    }
    private void OnElementShieldTypeChange(ElementType element)
    {
        if (_elementShield != null && _elementShield.Element == element ||
           _elementShield == null && element == ElementType.None)
            return;

        if (_elementShield != null && element == ElementType.None)
        {
            MonoObjectPool.Instance.PutObject(_elementShield.gameObject);
            _elementShield = null;
            return;
        }
        if (_elementShield == null)
            _elementShield = CreateElementShield();
        _elementShield?.SetColor(element);
    }
    void OnDestroy()
    {
        if (_elementShieldTypeArg != null)
            _elementShieldTypeArg.OnValueChange -= OnElementShieldTypeChange;
    }
    private ElementShield CreateElementShield()
    {
        var obj = MonoObjectPool.Instance.GetObject("ElementShield");
        if (obj == null)
        {
            Debug.LogError("【角色元素护盾显示】元素护盾创建失败");
            return null;
        }
        var es = obj.GetComponent<ElementShield>();
        if (es == null)
        {
            MonoObjectPool.Instance.PutObject(obj);
            Debug.LogError("【角色元素护盾显示】元素护盾组件获取失败");
            return null;
        }
        es.transform.SetParent(transform, false);
        es.transform.position = transform.TransformPoint(offset);
        return es;
    }
}