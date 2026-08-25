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
            typeof(ThrowStateMessage),
            typeof(ElementAttachmentMessage),
            typeof(ElementShieldViewStateMessage),
            typeof(FrozenStateMessage),
            typeof(DeathStateMessage)) { }
        public PlayerStateTransmitTransfer(params Type[] types)
        {
            foreach (Type type in types)
                if (type.IsAssignableTo(typeof(IPlayerStateMessage)))
                    _transmitMessageSet.Add(type);
                else
                    Debug.LogWarning($"【玩家状态转发】{type}不是玩家状态，注册失败");
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
            if (package.message is not IPlayerStateMessage stateMes)
            {
                Debug.LogWarning($"【玩家状态转发】{package.message?.GetType()}不是玩家状态");
                return;
            }
            stateMes.PlayerId = package.playerId;
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
