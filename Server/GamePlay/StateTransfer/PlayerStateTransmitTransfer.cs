#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerStateTransmitTransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        private HashSet<Type> _transmitMessageSet = new();
        public PlayerStateTransmitTransfer() : this(
            typeof(JumpStateMessage),
            typeof(ShootStateMessage),
            typeof(ThrowStateMessage)) { }
        public PlayerStateTransmitTransfer(params Type[] types)
        {
            foreach (Type type in types)
                _transmitMessageSet.Add(type);
        }
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);

            foreach (var mesType in _transmitMessageSet)
                AddListener(mesType, OnTransmitMessage);
        }
        public override void Stop()
        {
            foreach(var mesType in _transmitMessageSet)
                RemoveListener(mesType, OnTransmitMessage);
        }
        private void OnTransmitMessage(ClientPackage package)
        {
            if (package.message == null)
                return;
            var type = package.message.GetType();
            if (!_transmitMessageSet.Contains(type))
                return;
            var playerId = type.GetProperty("PlayerId");
            if (playerId == null)
            {
                Debug.LogWarning($"【玩家状态转发】状态{type}不存在PlayerId属性");
                return;
            }
            playerId.SetValue(package.message, package.playerId);
            foreach (int id in _playerSet)
            {
#if !LOCALDEBUG
                if (id == package.playerId)
                    continue;
#endif
                SendTo(new(id, SetHeader(), package.message));
            }
        }
    }
}
