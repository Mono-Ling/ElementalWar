#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class DynamicTextUITransfer : BaseTransfer
    {
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
                _playerSet.Add(player);
            AddListener<DynamicTextMessage>(OnDynamicText);
        }
        public override void Stop() => RemoveListener<DynamicTextMessage>(OnDynamicText);
        private void OnDynamicText(ClientPackage package)
        {
            if (package.message is not DynamicTextMessage stateMes)
                return;
            foreach (int id in _playerSet)
            {
#if !LOCALDEBUG
                if (id == package.playerId)
                    continue;
#endif
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(id, SetHeader(), stateMes));
                Console.WriteLine($"【文字UI】{stateMes.Text}");
            }
        }
    }
}
