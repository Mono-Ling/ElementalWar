using System.Collections;
using System.Collections.Generic;
using Space;
using UnityEngine;

/// <summary>
/// 空间管理物体
/// </summary>
public class SpaceItem
{
    public int spaceId { get; private set; } = -1;
    public AABB bound;
    // 业务成员

    /// <summary>
    /// 设置空间ID
    /// 插入空间八叉树时调用
    /// </summary>
    /// <param name="id">空间ID</param>
    public void SetSpaceID(int id) => spaceId = id;
}
