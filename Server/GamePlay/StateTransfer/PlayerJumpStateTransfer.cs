#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerJumpStateTransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);
            AddListener<JumpStateMessage>(OnJumpStateSyn);
            SetHeader();
        }
        public override void Stop() => RemoveListener<JumpStateMessage>(OnJumpStateSyn);
        private void OnJumpStateSyn(ClientPackage package)
        {
            if (package.message is not JumpStateMessage stateMes)
                return;
            stateMes.PlayerId = package.playerId;
            foreach (int id in _playerSet)
            {
#if !LOCALDEBUG
                if (id == package.playerId)
                    continue;
#endif
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(id,SetHeader(), stateMes));
            }
        }
    }
}
