using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerDeathTransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);
            AddListener<DeathMessage>(OnPlayerDeath);
        }
        public override void Stop()
            => RemoveListener<DeathMessage>(OnPlayerDeath);
        private void OnPlayerDeath(ClientPackage package)
        {
            if (package.message is not DeathMessage message)
                return;
            if(!_playerSet.Contains(package.playerId))
            {
                Debug.LogWarning("【玩家死亡中转】未知玩家");
                return;
            }
            if (package.playerId == message.AttackFromPlayerId)
                return;
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(message.AttackFromPlayerId, message));
            Debug.Log($"【玩家死亡中转】玩家{package.playerId}死亡");
        }
    }
}
