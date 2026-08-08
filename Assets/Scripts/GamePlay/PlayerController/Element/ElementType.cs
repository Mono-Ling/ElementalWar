using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ElementType
{
    None = 0,
    /// <summary>
    /// 火
    /// </summary>
    Fire = 1 << 0,
    /// <summary>
    /// 水
    /// </summary>
    Water = 1 << 1,
    /// <summary>
    /// 冰
    /// </summary>
    Ice = 1 << 2,
    /// <summary>
    /// 风
    /// </summary>
    Wind = 1 << 3,
    /// <summary>
    /// 雷
    /// </summary>
    Thunder = 1 << 4,
    /// <summary>
    /// 岩
    /// </summary>
    Rock = 1 << 5,
    /// <summary>
    /// 草
    /// </summary>
    Grass = 1 << 6,
}
