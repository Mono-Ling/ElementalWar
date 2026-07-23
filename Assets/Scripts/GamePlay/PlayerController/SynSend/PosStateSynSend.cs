using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosStateSynSend : BaseSynSend
{
    public float delayTime = 0.02f;
    private float _preTime;
    public override void OnUpdate()
    {
        if (Time.time - _preTime < delayTime)
            return;
        blackboard.GetValue<Vector3>("Position", out var pos);
        blackboard.GetValue<Quaternion>("Rotation", out var rot);
        blackboard.GetValue<float>("Pitch", out var pitch);
        Vector3Message posMessage = new() { X = pos.x, Y = pos.y, Z = pos.z };
        QuaternionMessage rotMessage = new() { X = rot.x, Y = rot.y, Z = rot.z, W = rot.w };
        PositionStateMessage posState = new() { Pos = posMessage, Rot = rotMessage, Pitch = pitch };
        Send(posState);
        _preTime = Time.time;
    }
}
