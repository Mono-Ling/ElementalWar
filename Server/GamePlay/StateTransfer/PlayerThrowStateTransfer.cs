#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerThrowStateTransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);
            AddListener<ThrowStateMessage>(OnThrowStateSyn);
            SetHeader();
        }
        public override void Stop() => RemoveListener<ThrowStateMessage>(OnThrowStateSyn);
        private void OnThrowStateSyn(ClientPackage package)
        {
            if (package.message is not ThrowStateMessage stateMes)
                return;
            stateMes.PlayerId = package.playerId;
            foreach (int id in _playerSet)
            {
#if !LOCALDEBUG
                if (id == package.playerId)
                    continue;
#endif
                SendTo(new(id, SetHeader(), stateMes));
            }
        }
    }
}
