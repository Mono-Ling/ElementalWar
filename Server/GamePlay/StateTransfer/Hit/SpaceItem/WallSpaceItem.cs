using System;
using System.Collections.Generic;
using System.Text;
using Space;

namespace Server.GamePlay.StateTransfer
{
    public class WallSpaceItem : SpaceItem
    {
        public int wallId {  get; private set; }
        public WallSpaceItem(int wallId,AABB bound)
        {
            this.wallId = wallId;
            this.bound = bound;
        }
    }
}
