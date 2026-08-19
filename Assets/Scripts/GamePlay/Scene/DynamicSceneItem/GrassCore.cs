using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using Message;
using UnityEngine;

public class GrassCore : BaseDynamicSceneItem
{
    [Header("本地创建位置偏移")]
    public Vector3 localOffset = new(0, 0.5f, 0);
    [Header("远程创建位置偏移")]
    public Vector3 remoteOffset = new(0.5f, 0, 0.5f);
    [Header("延时触发时间")]
    public float delayToTrigger = 10f;
    [Header("生成位置偏移半径")]
    public float createRadius = 0.5f;
    [Header("伤害半径")]
    public float damageRadius = 5f;
    [Header("伤害元素量")]
    [Range(0, ElementUtility.Content.STRONG)]
    public float damageElementContent = ElementUtility.Content.WEAK;
    [Header("伤害值")]
    public int damage = 60;
    [Header("列绽放")]
    public string burgeonName = "列绽放";
    public Color burgeonColor;
    public float burgeonDamageNum = 1.5f;
    [Header("超绽放")]
    public string hyperBloomName = "超绽放";
    public Color hyperBloomColor;
    public float hyperRadius = 7f;
    private BoxBound _boxBound;
    private Coroutine _coroutine;
    void Awake()
    {
        _boxBound = GetComponent<BoxBound>();
        if (_boxBound == null)
            Debug.LogError("【草原核】包围盒组件获取失败");
    }
    public override void LocalCreate(DynamicSceneItemType itemType) { }
    public override void LocalCreate<T>(DynamicSceneItemType itemType, T arg)
    {
        if (itemType != DynamicSceneItemType.GrassCore || arg is not Vector3 center)
        {
            DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
            return;
        }
        int id = GetDynamicSceneItemId;
        this.itemType = itemType;
        isRemote = false;
        dynamicSceneItemId = id;

        var angle = Random.Range(0, Mathf.PI + Mathf.PI);
        var r = Random.Range(0, createRadius);
        center += new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);
        center += localOffset;
        transform.position = center;
        _boxBound.UpdateBound();
        var bound = _boxBound.bound;

        BoundStateMessage boundMes = new() { Center = new(), Extents = new() };
        boundMes.Center.Switch(bound.center);
        boundMes.Extents.Switch(bound.extents);

        Any custom = Any.Pack(boundMes);
        DynamicItemStateMes mes = new()
        {
            DynamicItemId = id,
            ItemType = itemType,
            StateType = DynamicItemStateMes.Types.DynamicItemStateType.Create,
            CustomParams = custom,
        };
        SendTo(mes, true);
        _coroutine = StartCoroutine(DelayToTrigger());
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnElementAttackHit);
    }
    public override void LocalDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnElementAttackHit);
        base.LocalDestroy();
    }
    public override void OnRemoteCreate(NetReceiver netReceiver, DynamicItemStateMes mes)
    {
        base.OnRemoteCreate(netReceiver, mes);
        if (mes.CustomParams == null || !mes.CustomParams.Is(BoundStateMessage.Descriptor))
        {
            Debug.LogError("【草原核】自定义参数解析失败");
            return;
        }
        var boundMes = mes.CustomParams.Unpack<BoundStateMessage>();
        (Vector3 pos, _) = boundMes.Center;
        transform.position = pos + remoteOffset;
    }
    private IEnumerator DelayToTrigger()
    {
        yield return new WaitForSeconds(delayToTrigger);
        _coroutine = null;
        Trigger(damage);
    }
    private void OnElementAttackHit(NetPackage package)
    {
        if (package.message is not DynamicItemHitMessage message)
            return;
        if (message == null ||
            message.ElementAttack == null ||
            message.ClientDynamicItemId != dynamicSceneItemId)
            return;
        // if (!ElementUtility.TryToElementType(message.ElementAttack.ElementType, out var element))
        //     return;
        foreach (var elementMes in message.ElementAttack)
        {
            if (!ElementUtility.TryToElementType(elementMes.ElementType, out var element))
                continue;
            string text = default;
            Color color = default;
            switch (element)
            {
                case ElementType.Fire:
                    OnBurgeon();
                    text = burgeonName;
                    color = burgeonColor;
                    break;
                case ElementType.Thunder:
                    OnHyperBloom(message.FromPlayerId);
                    text = hyperBloomName;
                    color = hyperBloomColor;
                    break;
                default:
                    continue;
            }
            DynamicTextManager.Instance.LocalShowTextUI(new(text, color, transform.position));
            return;
        }
    }
    private void OnBurgeon()
    => Trigger(Mathf.CeilToInt(damage * burgeonDamageNum));
    private void OnHyperBloom(int maskPlayerId)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        HyperBloomRequestMessage hyperMes = new()
        {
            ClientDynamicItemId = dynamicSceneItemId,
            MaskPlayerId = maskPlayerId,
            Center = new(),
            Radius = hyperRadius,
            ElementAttack = new()
            {
                ElementType = ElementUtility.ToNumber(ElementType.Thunder),
                Content = damageElementContent,
                Damage = damage,
            }
        };
        hyperMes.Center.Switch(transform.position);
        SendTo(hyperMes, true);

        DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
    }
    private void Trigger(int damage)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }

        AreaElementDamageMes damageMes = new()
        {
            Center = new(),
            Radius = damageRadius
        };
        damageMes.Center.Switch(transform.position);
        damageMes.ElementAttack.Add(new ElementAttackMessage()
        {
            ElementType = ElementUtility.ToNumber(ElementType.Grass),
            Content = damageElementContent,
            Damage = damage,
        });
        SendTo(damageMes, true);

        DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);

        if (!ElementInfoMap.Instance.TryGetElementInfo(ElementType.Grass, out var info))
            return;
        var expEffArg = (transform.position, info.color);
        DynamicSceneItemMgr.Instance.
        CreateLocalDynamicSceneItem<(Vector3, Color)>(DynamicSceneItemType.GrenadeExp, expEffArg);
    }
}
