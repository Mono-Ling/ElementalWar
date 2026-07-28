using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosStateSynSend : BaseSynSend
{
    public float delayTime = 0.02f;
    private float _preTime;
    private PositionStateMessage _stateMes = new();
    private Vector3Message _posMes = new();
    private QuaternionMessage _rotMes = new();
    public override void OnUpdate()
    {
        if (Time.time - _preTime < delayTime)
            return;
        blackboard.GetValue<Vector3>("Position", out var pos);
        blackboard.GetValue<Quaternion>("Rotation", out var rot);
        blackboard.GetValue<float>("Pitch", out var pitch);
        // Vector3Message posMessage = new() { X = pos.x, Y = pos.y, Z = pos.z };
        // QuaternionMessage rotMessage = new() { X = rot.x, Y = rot.y, Z = rot.z, W = rot.w };
        // PositionStateMessage posState = new() { Pos = posMessage, Rot = rotMessage, Pitch = pitch };
        _posMes.Switch(pos);
        _rotMes.Switch(rot);

        _stateMes.Pos = _posMes;
        _stateMes.Rot = _rotMes;
        Send(_stateMes);
        _preTime = Time.time;
    }
}
