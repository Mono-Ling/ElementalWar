using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEff : MonoBehaviour
{
    private const float DELAY_EFF_DESTROY = 0.5f;
    private const float EFF_POS_EFFSET = 0.05f;
    private int _colorPropertyID = Shader.PropertyToID("_Color");
    private Renderer _renderer;
    private MaterialPropertyBlock _materialProperty;
    private WaitForSeconds _waitForSeconds = new(DELAY_EFF_DESTROY);
    void Awake()
    {
        _materialProperty = new();
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            Debug.LogError("【射击特效】渲染器获取失败");
    }
    public void ShowEff(Vector3 position, Vector3 normal, ElementType elementType)
    {
        transform.forward = normal;
        transform.position = position + normal.normalized * EFF_POS_EFFSET;

        if (ElementInfoMap.Instance.TryGetElementInfo(elementType, out var info))
        {
            _renderer.GetPropertyBlock(_materialProperty);
            _materialProperty.SetColor(_colorPropertyID, info.color);
            _renderer.SetPropertyBlock(_materialProperty);
        }
        else
            Debug.LogWarning($"【射击命中特效】元素信息获取失败{elementType}");
        StartCoroutine(DelayToDestroy());
    }
    private IEnumerator DelayToDestroy()
    {
        yield return new WaitForSeconds(DELAY_EFF_DESTROY);
        MonoObjectPool.Instance.PutObject(gameObject);
    }
}
