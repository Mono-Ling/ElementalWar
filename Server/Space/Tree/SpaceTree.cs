using System;
using System.Collections;
using System.Collections.Generic;
using Space;

public class SpaceTree
{
    private int _nextId;
    private Dictionary<int, SpaceDicItem> _itemDic = new();
    private SpaceTreeNode _root;
    public SpaceTree(AABB rootBound)
    => _root = new(null, 1, rootBound);

    public void Add(SpaceItem spaceItem)
    {
        TreeItem treeItem = new(_nextId, spaceItem.bound);
        if (Insert(treeItem))
        {
            SpaceDicItem dicItem = new(spaceItem.bound, spaceItem);
            spaceItem.SetSpaceID(_nextId);
            _itemDic.Add(_nextId++, dicItem);
        }
        else
            Console.WriteLine("【空间八叉树】插入失败");
    }
    public void Remove(SpaceItem spaceItem)
    {
        if (spaceItem.spaceId < 0 || !_itemDic.TryGetValue(spaceItem.spaceId, out var dicItem))
            return;
        TreeItem treeItem = new(spaceItem.spaceId, dicItem.currBound);
        var node = GetNode(treeItem);
        if (node != null)
            node.Remove(treeItem);
        else
            Console.WriteLine($"【空间八叉树】{spaceItem.spaceId}移除失败，在树中未找到");
        _itemDic.Remove(spaceItem.spaceId);
        spaceItem.SetSpaceID(-1);
    }
    public void UpdateItem(SpaceItem spaceItem)
    {
        if (spaceItem.spaceId < 0 || !_itemDic.TryGetValue(spaceItem.spaceId, out var dicItem))
            return;

        if (spaceItem.bound == dicItem.currBound)
            return;

        TreeItem oldItem = new(spaceItem.spaceId, dicItem.currBound);
        var node = GetNode(oldItem);
        if (node == null)
        {
            Console.WriteLine($"【空间八叉树】{spaceItem.spaceId}更新失败，原节点查找失败");
            return;
        }
        node.Remove(oldItem, false);

        TreeItem newItem = new(spaceItem.spaceId, spaceItem.bound);
        if (node.bound.IsContains(spaceItem.bound))
            node.Add(newItem);
        else
            if (!Insert(newItem))
            {
                _itemDic.Remove(spaceItem.spaceId);
                Console.WriteLine($"【空间八叉树】{spaceItem.spaceId}更新失败，新位置插入失败");
                spaceItem.SetSpaceID(-1);
                return;
            }
        _itemDic[spaceItem.spaceId] = new(newItem.bound, spaceItem);
    }
    /// <summary>
    /// 射线检测
    /// 返回最近命中
    /// </summary>
    /// <param name="ray">射线</param>
    /// <param name="hit">命中物体</param>
    /// <param name="distance">距离</param>
    /// <param name="hitMaskId">检测ID遮罩（-1全部纳入检测）</param>
    /// <returns></returns>
    public bool RayCast(Ray ray, out SpaceItem? hit, out float distance,int hitMaskId = -1)
    {
        float minDis = float.MaxValue;
        TreeItem treeItem = default(TreeItem);
        bool isHit = false;
        Stack<SpaceTreeNode> stack = new();
        stack.Push(_root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!ray.IntersectAABB(node.bound, out _))
                continue;
            foreach (var item in node.itemSet)
                if (ray.IntersectAABB(item.bound, out var currDis) && item.id != hitMaskId)
                    if (currDis < minDis)
                    {
                        minDis = currDis;
                        treeItem = item;
                        isHit = true;
                    }

            if (node.IsLeaf)
                continue;
            foreach (var child in node.childList)
                if (ray.IntersectAABB(child.bound, out _))
                    stack.Push(child);
        }

        SpaceItem? hitItem = null;
        if (isHit)
        {
            distance = minDis;
            if (_itemDic.TryGetValue(treeItem.id, out var dicItem))
                hitItem = dicItem.spaceItem;
            else
                Console.WriteLine($"【空间八叉树】{treeItem.id}字典中查找失败");
        }
        else
            distance = -1;
        hit = hitItem;
        return isHit;
    }
    public List<SpaceItem> BoxOverlap(AABB bound)
    => Overlap((aabb) => SpaceUtility.IsIntersect(bound, aabb));
    public List<SpaceItem> SphereOverlap(Sphere sphere)
    => Overlap((aabb) => SpaceUtility.IsIntersect(sphere, aabb));
    public List<SpaceItem> RayOverlap(Space.Ray ray)
    => Overlap((aabb) => SpaceUtility.IsIntersect(ray, aabb));
    private List<SpaceItem> Overlap(Func<AABB, bool> func)
    {
        List<TreeItem> itemList = new();
        Stack<SpaceTreeNode> stack = new();
        stack.Push(_root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!func.Invoke(node.bound))
                continue;
            foreach (var item in node.itemSet)
                if (func.Invoke(item.bound))
                    itemList.Add(item);

            if (node.IsLeaf)
                continue;
            foreach (var child in node.childList)
                if (func.Invoke(child.bound))
                    stack.Push(child);
        }
        List<SpaceItem> ans = new();
        foreach (var treeItem in itemList)
        {
            if (_itemDic.TryGetValue(treeItem.id, out var dicItem))
                ans.Add(dicItem.spaceItem);
            else
                Console.WriteLine($"【空间八叉树】{treeItem.id}字典中查找失败");
        }
        return ans;
    }
    /// <summary>
    /// 插入
    /// </summary>
    /// <param name="item">插入元素</param>
    /// <returns></returns>
    private bool Insert(TreeItem item)
    {
        if (!_root.bound.IsContains(item.bound))
            return false;
        AABB insertBound = item.bound;
        Stack<SpaceTreeNode> stack = new();
        stack.Push(_root);
        SpaceTreeNode targetNode = _root;
        float minVolume = _root.bound.Volume;
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!node.bound.IsContains(insertBound))
                continue;
            float currVolume = node.bound.Volume;
            if (currVolume < minVolume)
            {
                minVolume = currVolume;
                targetNode = node;
            }

            if (node.IsLeaf)
                continue;
            foreach (var child in node.childList)
                if (child.bound.IsContains(insertBound))
                    stack.Push(child);
        }
        targetNode.Add(item);
        return true;
    }
    /// <summary>
    /// 获取元素在树中的节点
    /// 不存在返回null
    /// </summary>
    /// <param name="item">查找元素</param>
    /// <returns></returns>
    private SpaceTreeNode? GetNode(TreeItem item)
    {
        if (!_root.bound.IsContains(item.bound))
        {
            Console.WriteLine($"【空间八叉树】超出树边界，无效Item{item.id}");
            return null;
        }
        var bound = item.bound;
        Stack<SpaceTreeNode> stack = new();
        stack.Push(_root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!node.bound.IsContains(bound))
                continue;
            if (node.itemSet.Contains(item))
                return node;

            if (node.IsLeaf)
                continue;
            foreach (var child in node.childList)
                if (child.bound.IsContains(bound))
                    stack.Push(child);
        }
        return null;
    }
}
