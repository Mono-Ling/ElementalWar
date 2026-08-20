using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Message;
using UnityEngine;
using Random = UnityEngine.Random;

public class ElementCrystal : BaseDynamicSceneItem
{
    public float delay = 20f;
    public float alpha = 0.7f;
    [Header("本地创建位置偏移")]
    public Vector3 localOffset = new(0, 0.3f, 0);
    [Header("生成位置偏移半径")]
    public float createNearRadius = 0.5f;
    public float createFarRadius = 2f;
    private int _colorPropertyID = Shader.PropertyToID("_Color");
    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private BoxBound _boxBound;
    private Coroutine _coroutine;
    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            Debug.LogError("【元素结晶】渲染器获取失败");
        _boxBound = GetComponent<BoxBound>();
        if (_boxBound == null)
            Debug.LogError("【元素结晶】包围盒获取失败");
        _propertyBlock = new();
    }
    public override void LocalCreate(DynamicSceneItemType itemType)
    => DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
    public override void LocalCreate<T>(DynamicSceneItemType itemType, T arg)
    {
        if (itemType != DynamicSceneItemType.ElementCrystal ||
           arg is not ValueTuple<ElementType, Vector3> value ||
           !ElementInfoMap.Instance.TryGetElementInfo(value.Item1, out var info))
        {
            DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
            return;
        }
        int id = GetDynamicSceneItemId;
        this.itemType = itemType;
        isRemote = false;
        dynamicSceneItemId = id;

        Color color = info.color;
        color = new(color.r, color.g, color.b, alpha);

        _renderer?.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_colorPropertyID, color);
        _renderer?.SetPropertyBlock(_propertyBlock);

        var angle = Random.Range(0, Mathf.PI + Mathf.PI);
        var r = Random.Range(createNearRadius, createFarRadius);
        var center = value.Item2;
        center += new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);
        center += localOffset;
        transform.position = center;
        _boxBound.UpdateBound();
        var bound = _boxBound.bound;

        BoundStateMessage boundMes = new() { Center = new(), Extents = new() };
        boundMes.Center.Switch(bound.center);
        boundMes.Extents.Switch(bound.extents);

        ElementCrystalInitMessage initMes = new()
        {
            Bound = boundMes,
            ElementType = ElementUtility.ToNumber(value.Item1),
        };

        Any custom = Any.Pack(initMes);
        DynamicItemStateMes mes = new()
        {
            DynamicItemId = id,
            ItemType = itemType,
            StateType = DynamicItemStateMes.Types.DynamicItemStateType.Create,
            CustomParams = custom,
        };
        SendTo(mes, true);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnTrigger);

        _coroutine = StartCoroutine(DelayToDestroy());
    }
    public override void LocalDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnTrigger);
        base.LocalDestroy();
    }
    public override void OnRemoteCreate(NetReceiver netReceiver, DynamicItemStateMes mes)
    {
        base.OnRemoteCreate(netReceiver, mes);
        if (!mes.CustomParams.TryUnpack<ElementCrystalInitMessage>(out var initMes))
            return;
        (var center, _) = initMes.Bound.Center;
        transform.position = center;

        if (!ElementUtility.TryToElementType(initMes.ElementType, out var element) ||
           !ElementInfoMap.Instance.TryGetElementInfo(element, out var info))
            return;

        Color color = info.color;
        color = new(color.r, color.g, color.b, alpha);

        _renderer?.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(_colorPropertyID, color);
        _renderer?.SetPropertyBlock(_propertyBlock);
    }
    private void OnTrigger(NetPackage package)
    {
        if (package.message is not OnTriggerMessage message ||
           message.ClientDynamicItemId != dynamicSceneItemId)
            return;
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
    }
    private IEnumerator DelayToDestroy()
    {
        yield return new WaitForSeconds(delay);
        _coroutine = null;
        DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
    }
}
