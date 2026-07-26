#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerShootStateTransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);
            AddListener<ShootStateMessage>(OnShootStateSyn);
            SetHeader();
        }
        public override void Stop() => RemoveListener<ShootStateMessage>(OnShootStateSyn);
        private void OnShootStateSyn(ClientPackage package)
        {

            if (package.message is not ShootStateMessage stateMes)
                return;
            stateMes.PlayerId = package.playerId;
            foreach(int id in _playerSet)
            {
#if !LOCALDEBUG
                if (id == package.playerId)
                    continue;
#endif
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo,new(id,SetHeader(),stateMes));
                Console.WriteLine("【射击状态中转】射击状态同步");
            }
        }
    }
}
