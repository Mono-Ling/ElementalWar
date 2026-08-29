using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosFeedbackReceive : BaseFeedbackReceive
{
    public float threshold = 0.5f;
    private Vector3 _targetPos;
    private bool _isAmend;
    private Rigidbody _rigidbody;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        _rigidbody = mainPlayer.GetComponent<Rigidbody>();
        if (_rigidbody == null)
            Debug.LogError("【位置反馈接收】刚体获取失败");
        AddListener<PlayerPosStateMesMap>(OnPositionMessage);
        _targetPos = Vector3.zero;
        _isAmend = false;
    }
    public override void OnFixedUpdate()
    {
        if (!_isAmend || _rigidbody == null)
            return;
        _rigidbody.position = _targetPos;
        _rigidbody.velocity = Vector3.zero;
        _isAmend = false;
    }
    public override void OnRemove()
    => RemoveListener<PlayerPosStateMesMap>(OnPositionMessage);
    private void OnPositionMessage(PlayerPosStateMesMap message)
    {
        if (message == null)
            return;
        if (!blackboard.GetValue("PlayerId", out int playerId))
            return;
        if (!message.PlayerPosStateMap.TryGetValue(playerId, out var posMes))
            return;
        (var pos, _) = posMes.Pos;
        if (blackboard.GetValue("Position", out _targetPos))
            if ((_targetPos - pos).magnitude > threshold)
            {
                _targetPos = pos;
                _isAmend = true;
            }
    }
}