using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Event
{
    public enum EventType
    {
        /// <summary>
        /// ClientPackage类型参数
        /// </summary>
        OnReceive,
        /// <summary>
        /// ClientPackage类型参数
        /// </summary>
        SendTo,
        /// <summary>
        /// int类型参数
        /// playerID
        /// </summary>
        OnPlayerConnect,
        /// <summary>
        /// int类型参数
        /// playerID
        /// </summary>
        OnPlayerDisconnect,
        /// <summary>
        /// DynamicSceneItem类型参数
        /// </summary>
        OnDynamicSceneItemAdd,
        /// <summary>
        /// DynamicSceneItem类型参数
        /// </summary>
        OnDynamicSceneItemRemove,
    }
}
