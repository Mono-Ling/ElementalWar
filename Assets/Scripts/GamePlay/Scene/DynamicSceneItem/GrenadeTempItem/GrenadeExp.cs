using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Message;
using UnityEngine;

public class GrenadeExp : BaseDynamicSceneItem
{
    public float totalTime = 1f;
    [SerializeField]
    private float _progress;
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    private int _colorPropertyIndex = Shader.PropertyToID("_Color");
    private int _progressPropertyIndex = Shader.PropertyToID("_Progress");
    private int _centerPointPropertyIndex = Shader.PropertyToID("_CenterPoint");

    private GrenadeExpInitMes _customMes = new() { Pos = new(), Color = new() };
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            Debug.LogError("【手榴弹爆炸特效】渲染器获取失败");
        _propertyBlock = new();
    }
    private void Init(Vector3 position, Color color, Action destroyCallback = null)
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

        StartCoroutine(DelayToDestroy(destroyCallback));
    }
    private IEnumerator DelayToDestroy(Action action = null)
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
        action?.Invoke();
    }
    public override void OnRemoteCreate(NetReceiver netReceiver, DynamicItemStateMes mes)
    {
        base.OnRemoteCreate(netReceiver, mes);
        if (mes.CustomParams == null || !mes.CustomParams.Is(GrenadeExpInitMes.Descriptor))
        {
            Debug.LogError("【手榴弹爆炸特效】自定义参数解析失败");
            return;
        }
        var customMes = mes.CustomParams.Unpack<GrenadeExpInitMes>();
        (Color color, _) = customMes.Color;
        (Vector3 pos, _) = customMes.Pos;
        Init(pos, color);
    }
    public override void LocalCreate(DynamicSceneItemType itemType) { }
    public override void LocalCreate<T>(DynamicSceneItemType itemType, T arg)
    {
        if (arg is ValueTuple<Vector3, Color> value)
            ManualCreate(value.Item1, value.Item2);
        else
            Debug.LogError($"【手榴弹爆炸特效】参数化创建参数类型不匹配，期望({typeof(Vector3)}, {typeof(Color)})");
    }
    private void ManualCreate(Vector3 position, Color color)
    {
        int id = GetDynamicSceneItemId;
        this.itemType = DynamicSceneItemType.GrenadeExp;

        _customMes.Pos.Switch(position);
        _customMes.Color.Switch(color);
        Any custom = Any.Pack(_customMes);

        DynamicItemStateMes mes = new()
        {
            DynamicItemId = id,
            ItemType = itemType,
            StateType = DynamicItemStateMes.Types.DynamicItemStateType.Create,
            CustomParams = custom,
        };
        SendTo(mes, true);

        isRemote = false;
        dynamicSceneItemId = id;

        Init(position, color, () =>
        {
            DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
        });
    }
}
