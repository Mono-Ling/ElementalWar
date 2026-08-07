using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class BoundStateSynSend : BaseSynSend
{
    public float delayTime = 0.02f;
    private float _preTime;
    private BoxBound _boxBound;
    private BoundStateMessage _stateMes = new();
    private Vector3Message _centerMes = new();
    private Vector3Message _extentsMes = new();
    public override void Init(MainPlayerNetSyn mainPlayer)
    {
        base.Init(mainPlayer);
        _boxBound = mainPlayer.gameObject.GetComponent<BoxBound>();
        if (_boxBound == null)
            Debug.LogError("【包围盒状态同步发送】包围盒组件获取失败");
    }
    public override void OnUpdate()
    {
        if (Time.time - _preTime < delayTime)
            return;
        Vector3 center = _boxBound.bound.center;
        Vector3 extents = _boxBound.bound.extents;
        _centerMes.Switch(center);
        _extentsMes.Switch(extents);

        _stateMes.Center = _centerMes;
        _stateMes.Extents = _extentsMes;

        Send(_stateMes);
        _preTime = Time.time;
    }
}
