using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementShield : MonoBehaviour
{
    public ElementType Element => _element;
    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperty;
    private int _colorPropertyID = Shader.PropertyToID("_Color");
    private ElementType _element;
    void Awake()
    {
        _materialProperty = new();
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            Debug.LogError("【元素护盾】渲染器获取失败");
    }
    public void SetColor(ElementType element)
    {
        if (!ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            return;
        _element = element;
        _renderer?.GetPropertyBlock(_materialProperty);
        _materialProperty.SetColor(_colorPropertyID, info.color);
        _renderer?.SetPropertyBlock(_materialProperty);
    }
}
