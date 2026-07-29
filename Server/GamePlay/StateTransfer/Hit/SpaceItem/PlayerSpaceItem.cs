using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerSpaceItem : SpaceItem
    {
        public int playerId { get;private set;  }
        public DateTime preTime;
        public PlayerSpaceItem(int playerId) => this.playerId = playerId;
    }
}
