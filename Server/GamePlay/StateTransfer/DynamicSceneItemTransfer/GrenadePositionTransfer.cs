#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class GrenadePositionTransfer : BaseTransfer
    {
        private HashSet<DynamicSceneItem> _grenadeSet = new();
        private List<int>? _playerList;
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            _playerList = playerList;

            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicAdd);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicRemove);

            AddListener<GrenadePositionMessage>(OnGrenadeSyn);
        }
        public override void Stop()
        {
            RemoveListener<GrenadePositionMessage>(OnGrenadeSyn);

            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicAdd);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicRemove);
        }
        private void OnDynamicAdd(DynamicSceneItem item)
        {
            if (item.itemType == DynamicSceneItemType.Grenade)
            {
                _grenadeSet.Add(item);
            }
        }
        private void OnDynamicRemove(DynamicSceneItem item)
        {
            if (item.itemType == DynamicSceneItemType.Grenade)
            {
                _grenadeSet.Remove(item);
            }
        }
        private void OnGrenadeSyn(ClientPackage package)
        {
            if (package.message is not GrenadePositionMessage message)
                return;
            DynamicSceneItem item = new(package.playerId, message.DynamicItemId, default, DynamicSceneItemType.Grenade);
            if (!_grenadeSet.TryGetValue(item, out var foundItem))
                return;

            message.DynamicItemId = foundItem.dynamicItemId;

            foreach (var playerId in _playerList!)
            {
#if !LOCALDEBUG
                if (playerId == package.playerId)
                    continue;
#endif

                SendTo(new(playerId, SetHeader(false), message));
            }
        }
    }
}
