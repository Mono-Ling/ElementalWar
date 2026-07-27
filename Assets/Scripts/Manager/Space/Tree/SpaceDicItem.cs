using System.Collections;
using System.Collections.Generic;
using Space;

public struct SpaceDicItem
{
    public AABB currBound;
    public SpaceItem spaceItem;
    public SpaceDicItem(AABB currBound, SpaceItem spaceItem)
    {
        this.currBound = currBound;
        this.spaceItem = spaceItem;
    }
}
