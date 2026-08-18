using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Space;

namespace Server.GamePlay.AttackRequest
{
    public struct PlayerExpHitReq : IExplosionHitReq
    {
        public Sphere range { get; set; }
        public ElementAttackMessage elementAttack { get; set; }
        public int fromPlayerId { get; set; }
        public long tick { get; set; }
    }
}
