using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    /// <summary>
    /// NetPackage类型参数
    /// 消息接收回调
    /// </summary>
    OnReceive,
    /// <summary>
    /// NetPackage类型参数
    /// </summary>
    SendTo,
    /// <summary>
    /// 无参
    /// 玩家死亡回调
    /// </summary>
    OnPlayerDeath,
    /// <summary>
    /// 无参
    /// 玩家复活回调
    /// </summary>
    OnPlayerReset,
}
