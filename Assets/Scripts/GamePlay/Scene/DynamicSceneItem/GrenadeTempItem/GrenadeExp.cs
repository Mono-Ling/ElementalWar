using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeExp : MonoBehaviour
{
    public float totalTime = 1f;
    [SerializeField]
    private float _progress;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    private int _colorPropertyIndex = Shader.PropertyToID("_Color");
    private int _progressPropertyIndex = Shader.PropertyToID("_Progress");
    private int _centerPointPropertyIndex = Shader.PropertyToID("_CenterPoint");
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            Debug.LogError("【手榴弹爆炸特效】渲染器获取失败");
        _propertyBlock = new();
        Init(Vector3.zero, Color.red);
    }
    public void Init(Vector3 position, Color color)
    {
        if (_renderer == null)
        {
            Debug.LogError("【手榴弹爆炸特效】渲染器空引用");
            return;
        }

        transform.position = position;
        _progress = 0;

        _renderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock?.SetColor(_colorPropertyIndex, color);
        _propertyBlock?.SetVector(_centerPointPropertyIndex, position);
        _propertyBlock?.SetFloat(_progressPropertyIndex, _progress);
        _renderer.SetPropertyBlock(_propertyBlock);

        StartCoroutine(DelayToDestroy());
    }
    private IEnumerator DelayToDestroy()
    {
        totalTime = Mathf.Max(totalTime, 0.001f);
        float startTime = Time.time;
        while (Time.time - startTime < totalTime)
        {
            _progress = (Time.time - startTime) / totalTime;

            _renderer?.GetPropertyBlock(_propertyBlock);
            _propertyBlock?.SetFloat(_progressPropertyIndex, _progress);
            _renderer?.SetPropertyBlock(_propertyBlock);

            yield return null;
        }
        MonoObjectPool.Instance.PutObject(gameObject);
    }
}
