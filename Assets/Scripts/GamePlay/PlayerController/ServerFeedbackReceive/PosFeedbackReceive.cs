using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class PosFeedbackReceive : BaseFeedbackReceive
{
    private bool _isInit;
    public float smoothTime = 0.1f;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        AddListener<PlayerPosStateMesMap>(OnPositionMessage);
    }
    public override void OnRemove()
    => RemoveListener<PlayerPosStateMesMap>(OnPositionMessage);
    private void OnPositionMessage(PlayerPosStateMesMap message)
    {
        if (message == null || _isInit)
            return;
        if (!blackboard.GetValue("PlayerId", out int playerId))
            return;
        if (!message.PlayerPosStateMap.TryGetValue(playerId, out var posMes))
            return;
        (var pos, _) = posMes.Pos;
        mainPlayer.transform.position = pos;
        _isInit = true;
    }
}
