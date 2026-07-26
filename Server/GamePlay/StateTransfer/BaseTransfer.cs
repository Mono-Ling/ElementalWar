using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Message;

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
    }
}
