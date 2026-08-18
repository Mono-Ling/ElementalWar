using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Space;

namespace Server.GamePlay.AttackRequest
{
    public struct ShootHitReq : IAttackRequest
    {
        public int fromPlayerId { get; set; }
        public ElementAttackMessage elementAttack;
        public long tick {  get; set; }
        public int maskSpaceId;
        public Ray ray;
        public ShootHitReq(int playerId, Ray ray, ElementAttackMessage elementAttack, int maskSpaceId, long tick)
        {
            this.fromPlayerId = playerId;
            this.elementAttack = elementAttack;
            this.ray = ray;
            this.maskSpaceId = maskSpaceId;
            this.tick = tick;
        }
    }
}
