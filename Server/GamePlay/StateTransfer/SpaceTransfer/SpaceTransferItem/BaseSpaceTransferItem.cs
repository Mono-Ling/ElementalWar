using System;
using System.Collections.Generic;
using System.Text;
using Message;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public abstract class BaseSpaceTransferItem : BaseTransfer
    {
        protected Dictionary<int, PlayerSpaceItem>? playerSpaceItemDic;
        protected SpaceTree? spaceTree;
        public virtual void Start(Dictionary<int, PlayerSpaceItem>? playerSpaceItemDic, SpaceTree? spaceTree)
        {
            this.playerSpaceItemDic = playerSpaceItemDic;
            this.spaceTree = spaceTree;
        }
    }
}
