using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Space;
using static Message.AreaElementDamageMes.Types;

namespace Server.GamePlay.AttackRequest
{
    public struct AreaElementDamageReq : IAttackRequest
    {
        public int fromPlayerId { get; set; }
        public long tick { get; set; }
        public int maskSpaceId;
        public AreaElementDamageMes damageMes;
        public Sphere area;
        public AOEType aoeType;
    }
}
