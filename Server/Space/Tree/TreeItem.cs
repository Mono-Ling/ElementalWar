using System.Collections;
using System.Collections.Generic;
using Space;

public struct TreeItem
{
    public int id { get; private set; }
    public AABB bound { get; private set; }
    public TreeItem(int id, AABB bound)
    {
        this.id = id;
        this.bound = bound;
    }
}
