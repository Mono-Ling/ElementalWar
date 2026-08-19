using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Space;

namespace Server.GamePlay.AttackRequest
{
    public struct HyperBloomHitReq : IAttackRequest
    {
        public int fromPlayerId { get; set; }
        public long tick { get; set; }
        public int maskPlayerId;
        public Sphere range;
        public ElementAttackMessage attackMessage;
        public HyperBloomHitReq(int maskPlayer, long tick, Sphere range, ElementAttackMessage attackMessage)
        {
            this.maskPlayerId = maskPlayer;
            this.tick = tick;
            this.range = range;
            this.attackMessage = attackMessage;
            fromPlayerId = -1;
        }
    }
}
