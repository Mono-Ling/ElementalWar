using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class Grenade : BaseDynamicSceneItem
{
    private const string EXP_EFF_PATH = "GrenadeExp";
    public float delayDestroy = 5f;
    public float delaySend = 0.02f;
    public float positionSmoothTime = 0.06f;
    private Rigidbody _rigidbody;
    private Vector3 _targetPos;
    private Vector3 _smoothedPos;
    private Vector3 _posSmoothRef;

    private bool _isInitRemoteSyn = false;
    private DateTime _preTime;

    private Vector3Message _posMes = new();
    private GrenadePositionMessage _grenadeMes = new();
    private Color _expEffColor = Color.blue;
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
            Debug.LogError("【手榴弹】刚体获取失败");
    }
    #region Local
    public void Fire(Vector3 pos, Vector3 force)
    {
        transform.position = pos;
        _rigidbody.AddForce(force, ForceMode.Impulse);

        StartCoroutine(DelayToDestroy(() =>
        {
            Debug.Log("Local Grenade Exp");
            DynamicSceneItemMgr.Instance.DestroyLocalDynamicSceneItem(this);
        }, _expEffColor));
    }
    void LateUpdate()
    {
        if (isRemote)
            return;
        if ((DateTime.UtcNow - _preTime).TotalSeconds < delaySend)
            return;

        _posMes.Switch(transform.position);
        _grenadeMes.DynamicItemId = dynamicSceneItemId;
        _grenadeMes.Pos = _posMes;
        _grenadeMes.Time = DateTime.UtcNow.Ticks;

        SendTo(_grenadeMes);

        _preTime = DateTime.UtcNow;
    }
    public override void LocalDestroy()
    {
        base.LocalDestroy();
        Reset();
    }
    #endregion
    #region  Remote
    public override void OnRemoteCreate(NetReceiver netReceiver, DynamicItemStateMes mes)
    {
        base.OnRemoteCreate(netReceiver, mes);
        _isInitRemoteSyn = true;
        _rigidbody.isKinematic = true;
        AddListener<GrenadePositionMessage>(OnGrenadePosMes);
    }
    private void OnGrenadePosMes(GrenadePositionMessage mes)
    {
        if (mes == null || mes.DynamicItemId != dynamicSceneItemId)
            return;
        DateTime remoteTime = new(mes.Time);
        if (remoteTime < _preTime)
            return;

        _preTime = remoteTime;
        (Vector3 remotePos, _) = mes.Pos;
        if (_isInitRemoteSyn)
        {
            _smoothedPos = remotePos;
            transform.position = remotePos;
        }
        _targetPos = remotePos;
        _isInitRemoteSyn = false;
    }
    public override void OnRemoteDestroy(DynamicItemStateMes mes)
    {
        RemoveListener<GrenadePositionMessage>(OnGrenadePosMes);
        Reset();
    }
    void FixedUpdate()
    {
        if (!isRemote)
            return;
        _smoothedPos = Vector3.SmoothDamp(_smoothedPos, _targetPos,
                        ref _posSmoothRef, positionSmoothTime);
        _rigidbody.MovePosition(_smoothedPos);
    }
    #endregion
    #region General
    private IEnumerator DelayToDestroy(Action action, Color effColor)
    {
        yield return new WaitForSeconds(delayDestroy);

        CreateExpEff(effColor);

        action?.Invoke();
    }
    private void CreateExpEff(Color effColor)
    {
        var item = DynamicSceneItemMgr.Instance.CreateLocalDynamicSceneItem(DynamicSceneItemType.GrenadeExp);
        if (item is GrenadeExp exp)
            exp.ManualCreate(_rigidbody.position, effColor);
    }
    private void Reset()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.velocity = Vector3.zero;
    }
    #endregion
}
