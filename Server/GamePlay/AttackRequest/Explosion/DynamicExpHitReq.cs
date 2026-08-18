using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.GamePlay.StateTransfer;
using Space;

namespace Server.GamePlay.AttackRequest
{
    public struct DynamicExpHitReq : IExplosionHitReq
    {
        public Sphere range { get; set; }
        public ElementAttackMessage elementAttack { get; set; }
        public int fromPlayerId { get; set; }
        public long tick { get; set; }

        public DynamicSceneItem dynamicItem;
        public DynamicExpHitReq(DynamicSceneItem item, ElementAttackMessage elementAttack, Sphere range, long tick)
        {
            this.dynamicItem = item;
            this.range = range;
            this.elementAttack = elementAttack;
            this.tick = tick;
            this.fromPlayerId = item.playerId;
        }
    }
}
