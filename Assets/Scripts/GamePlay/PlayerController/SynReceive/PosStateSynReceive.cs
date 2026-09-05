using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosStateSynReceive : BaseSynReceive
{
    public Vector3 posOffset;
    private DateTime _preTime;
    public override void Init(OtherPlayer otherPlayer, Dictionary<int, Blackboard> blackboardDic)
    {
        base.Init(otherPlayer, blackboardDic);
        AddListener<PlayerPosStateMesMap>(OnPlayerPosStateMesMap);
    }
    private void OnPlayerPosStateMesMap(PlayerPosStateMesMap mesMap)
    {
        if (mesMap == null)
            return;
        DateTime localTime = new(mesMap.ServerTime);
        if (localTime < _preTime)
            return;
        _preTime = localTime;

        foreach (var item in mesMap.PlayerPosStateMap)
        {
            if (blackboardDic.TryGetValue(item.Key, out var blackboard))
                PositionStateSyn(blackboard, item.Value);
            // else
            //     Debug.LogWarning($"【位置同步接收器】不存在网络玩家{item.Key}");
        }
    }
    private void PositionStateSyn(Blackboard blackboard, PositionStateMessage message)
    {
        if (message == null)
            return;
        Vector3 pos = new(message.Pos.X, message.Pos.Y, message.Pos.Z);
        pos += posOffset;
        Quaternion rot = new(message.Rot.X, message.Rot.Y, message.Rot.Z, message.Rot.W);
        blackboard.SetValue<Vector3>("Position", pos);
        blackboard.SetValue<Quaternion>("Rotation", rot);
        blackboard.SetValue<float>("Pitch", message.Pitch);
    }
    public override void OnRemove()
    => RemoveListener<PlayerPosStateMesMap>(OnPlayerPosStateMesMap);
}