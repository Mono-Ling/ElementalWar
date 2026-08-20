using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Message;
using Server.Event;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer.Hit.TriggerItem
{
    public abstract class TriggerItem
    {
        public bool IsEnable => isEnable;
        public AABB Bound => bound;
        protected DynamicSceneItem dynamicSceneItem;
        protected AABB bound;
        protected bool isEnable;
        public virtual bool InitTriggerItem(DynamicSceneItem item,ITriggerItemInitMessage message)
        {
            dynamicSceneItem = item;
            if (message == null)
                return false;
            (var center, _) = message.Bound.Center;
            (var extents, _) = message.Bound.Extents;
            bound = new(center, extents);
            isEnable = true;
            return true;
        }
        public virtual void OnTrigger()
        {
            OnTriggerMessage triggerMes = new() { ClientDynamicItemId = dynamicSceneItem.clientDynamicItemId };
            UdpHeader udpHeader = new() { IsResponse = true };
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(dynamicSceneItem.playerId, udpHeader, triggerMes));
        }
        public static TriggerItem? GetTriggerItem(DynamicSceneItemType type) => type switch
        {
            DynamicSceneItemType.ElementCrystal => new ElementCrystalTriggerItem(),
            _ => null,
        };
        public static bool GetTriggerItemInitMessage(DynamicSceneItemType type,Any? any,ref ITriggerItemInitMessage? message)
        => type switch
        {
            DynamicSceneItemType.ElementCrystal => TryUnpackAndAssign<ElementCrystalInitMessage>(any,ref message),
            _ => false,
        };
        private static bool TryUnpackAndAssign<T>(Any? any, ref ITriggerItemInitMessage? message)
        where T : class, ITriggerItemInitMessage,IMessage,new()
        {
            if (any?.TryUnpack<T>(out var result) ?? false)
            {
                message = result;
                return true;
            }
            return false;
        }
    }
}
