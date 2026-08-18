using System;
using System.Collections.Generic;
using System.Text;
using Message;

namespace Server.GamePlay.AttackRequest
{
    public interface IAttackRequest
    {
        public int fromPlayerId { get; set; }
        public long tick { get; set; }
    }
}
