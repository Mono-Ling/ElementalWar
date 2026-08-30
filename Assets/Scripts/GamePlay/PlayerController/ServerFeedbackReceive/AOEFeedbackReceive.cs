using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class AOEFeedbackReceive : BaseFeedbackReceive
{
    private ElementReceiver _elementReceiver;
    public override void Init(MainPlayerNetSyn mainPlayer, Blackboard blackboard)
    {
        base.Init(mainPlayer, blackboard);
        _elementReceiver = mainPlayer.GetComponent<ElementReceiver>();
        AddListener<AreaElementDamageMes>(OnAreaDamage);
    }
    public override void OnRemove()
    => RemoveListener<AreaElementDamageMes>(OnAreaDamage);
    private void OnAreaDamage(AreaElementDamageMes mes)
    {
        if (mes == null)
            return;
        foreach (var attack in mes.ElementAttack)
        {
            if (!ElementUtility.TryToElementType(attack.ElementType, out var element))
                continue;
            _elementReceiver?.ReceiveElement(element, attack.Content, attack.Damage, attack.FromPlayerId);
            Debug.Log($"【玩家受元素范围伤害】{element}");
        }
        (var pos, _) = mes.Center;
        var dir = pos - mainPlayer.transform.position;
        HitDir.ShowHitDir(dir, mainPlayer.transform.forward);
    }
}
