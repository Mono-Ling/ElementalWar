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
    /// float类型
    /// </summary>
    CameraPitchDelta,
    /// <summary>
    /// float类型[-1,1]
    /// </summary>
    OnCameraPitchChange,
}
