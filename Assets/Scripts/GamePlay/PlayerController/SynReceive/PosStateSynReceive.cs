using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosStateSynReceive : BaseSynReceive
{
    public Vector3 posOffset;
    private DateTime _preTime;
    public override void Init(OtherPlayer otherPlayer, Blackboard blackboard)
    {
        base.Init(otherPlayer, blackboard);
        AddListener<PositionStateMessage>(OnPositionStateSyn);
    }
    private void OnPositionStateSyn(PositionStateMessage message)
    {
        if (message == null)
            return;
        DateTime localTime = new(message.LocalTime);
        if (localTime < _preTime)
            return;
        _preTime = localTime;

        Vector3 pos = new(message.Pos.X, message.Pos.Y, message.Pos.Z);
        pos += posOffset;
        Quaternion rot = new(message.Rot.X, message.Rot.Y, message.Rot.Z, message.Rot.W);
        blackboard.SetValue<Vector3>("Position", pos);
        blackboard.SetValue<Quaternion>("Rotation", rot);
        blackboard.SetValue<float>("Pitch", message.Pitch);
    }
    public override void OnRemove()
    => RemoveListener<PositionStateMessage>(OnPositionStateSyn);
}
