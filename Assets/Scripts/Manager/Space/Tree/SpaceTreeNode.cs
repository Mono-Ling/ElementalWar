using System.Collections;
using System.Collections.Generic;
using System.Data;
using Space;
using UnityEngine;

public class SpaceTreeNode
{
    private const int MAX_DEPTH = 8;
    private const int MAX_ITEM_COUNT = 4;
    /// <summary>
    /// 最小分裂有效子节点数
    /// </summary>
    private const int MIN_SPLIT_COUNT = 2;
    public int ItemCount => itemSet.Count;
    public bool IsLeaf => childList == null;
    public AABB bound { get; private set; }
    public int depth { get; private set; }
    public HashSet<TreeItem> itemSet { get; private set; } = new();
    public List<SpaceTreeNode> childList { get; private set; }
    public SpaceTreeNode parent { get; private set; }
    public SpaceTreeNode(SpaceTreeNode parent, int depth, AABB bound)
    {
        this.parent = parent;
        this.depth = depth;
        this.bound = bound;
    }
    public void Add(TreeItem item)
    {
        itemSet.Add(item);
        TrySplit();
    }
    public void Remove(TreeItem item)
    {
        itemSet.Remove(item);
        TryMerge();
        parent?.TryMerge();
    }
    public bool TrySplit()
    {
        if (!IsLeaf || ItemCount < MAX_ITEM_COUNT || depth >= MAX_DEPTH)
            return false;
        AABB[] childBound = new AABB[8];
        Vector3 childSize = this.bound.extents / 2;
        Vector3 pivot = this.bound.center;
        for (int i = 0; i < 8; i++)
        {
            Vector3 childPivot = pivot + new Vector3((i & 1) == 0 ? -childSize.x : childSize.x,
                                                     (i & 2) == 0 ? -childSize.y : childSize.y,
                                                     (i & 4) == 0 ? -childSize.z : childSize.z);
            childBound[i] = new AABB(childPivot, childSize);
        }

        int filledChildCount = 0;
        for (int i = 0; i < 8; i++)
        {
            foreach (var item in itemSet)
                if (childBound[i].IsContains(item.bound))
                {
                    filledChildCount++;
                    break;
                }
            if (filledChildCount >= MIN_SPLIT_COUNT)
                break;
        }
        if (filledChildCount < MIN_SPLIT_COUNT)
            return false;

        CreateChilds(childBound);

        return true;
    }
    public bool TryMerge()
    {
        if (IsLeaf)
            return false;

        int totalCount = ItemCount;
        foreach (var child in childList)
        {
            if (!child.IsLeaf)
                return false;
            totalCount += child.ItemCount;
        }

        if (totalCount > MAX_ITEM_COUNT / 2)
            return false;

        DestroyChild();

        return true;
    }
    private void CreateChilds(AABB[] childBound)
    {
        List<SpaceTreeNode> childs = new(8);
        for (int i = 0; i < childBound.Length; i++)
            childs.Add(new(this, this.depth + 1, childBound[i]));

        List<TreeItem> lostItemList = new();
        for (int i = 0; i < 8; i++)
        {
            foreach (var item in itemSet)
                if (childs[i].bound.IsContains(item.bound))
                {
                    childs[i].Add(item);
                    lostItemList.Add(item);
                }

            foreach (var loseItem in lostItemList)
                itemSet.Remove(loseItem);
            lostItemList.Clear();

            if (itemSet.Count == 0)
                break;
        }
        this.childList = childs;
    }
    private void DestroyChild()
    {
        foreach (var child in childList)
        {
            foreach (var item in child.itemSet)
                this.itemSet.Add(item);
        }
        childList = null;
    }
}
