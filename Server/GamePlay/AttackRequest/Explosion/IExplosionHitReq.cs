using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Space;

namespace Server.GamePlay.AttackRequest
{
    public interface IExplosionHitReq : IAttackRequest
    {
        public Sphere range {  get; set; }
        public ElementAttackMessage elementAttack { get; set; }
    }
}
