using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public abstract class BaseTransfer
    {
        protected PlayerStateTransfer? playerStateTransfer;
        protected UdpHeader udpHeader = new();
        public virtual void Start(PlayerStateTransfer? playerStateTransfer,List<int> playerList)
            => this.playerStateTransfer = playerStateTransfer;
        public virtual void Update() { }
        public virtual void Stop() { }
        protected UdpHeader SetHeader(bool isResponse = true)
        {
            UdpHeader header = new();
            header.IsResponse = isResponse;
            return header;
        }
        protected void AddListener<T>(Action<ClientPackage> action) where T : IMessage
            => playerStateTransfer?.AddListener<T>(action);
        protected void RemoveListener<T>(Action<ClientPackage> action) where T : IMessage
            => playerStateTransfer?.RemoveListener<T>(action);
        protected void AddListener(Type type, Action<ClientPackage> action)
            => playerStateTransfer?.AddListener(type, action);
        protected void RemoveListener(Type type, Action<ClientPackage> action)
            => playerStateTransfer?.RemoveListener(type, action);
        protected void SendTo(ClientPackage message)
            => EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, message);
    }
}
