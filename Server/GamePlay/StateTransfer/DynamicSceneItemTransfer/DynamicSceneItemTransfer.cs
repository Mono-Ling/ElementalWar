#define LOCALDEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public struct DynamicSceneItem
    {
        public int playerId { get; private set; }
        public int clientDynamicItemId { get; private set; }
        public int dynamicItemId { get; private set; }
        public DynamicSceneItemType itemType { get; private set; }
        public DynamicSceneItem(int playerId, int clientDynamicItemId, int dynamicItemId, DynamicSceneItemType itemType)
        {
            this.playerId = playerId;
            this.clientDynamicItemId = clientDynamicItemId;
            this.dynamicItemId = dynamicItemId;
            this.itemType = itemType;
        }
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not DynamicSceneItem item)
                return false;
            return playerId == item.playerId
                && clientDynamicItemId == item.clientDynamicItemId
                && itemType == item.itemType;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(playerId, clientDynamicItemId, itemType);
        }
    }
    public class DynamicSceneItemTransfer : BaseTransfer
    {
        private int _nextDynamicItemId;
        private Dictionary<(int playerId, int clientDynamicItemId), int> _dynamicItemDic = new();
        private HashSet<int> _playerSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (var player in playerList)
            {
                if (_playerSet.Contains(player))
                {
                    Console.WriteLine($"【场景动态物体中转】{player}重复注册");
                    continue;
                }
                _playerSet.Add(player);
            }
            AddListener<DynamicItemStateMes>(OnDynamicStateMes);
        }
        public override void Stop()
        {
            RemoveListener<DynamicItemStateMes>(OnDynamicStateMes);
        }
        private void OnDynamicStateMes(ClientPackage package)
        {
            if (!_playerSet.Contains(package.playerId))
                return;
            if (package.message is not DynamicItemStateMes message)
                return;
            var key = (package.playerId, message.DynamicItemId);

            switch(message.StateType)
            {
                case DynamicItemStateMes.Types.DynamicItemStateType.Create:
                    OnDynamicCreateMes(key, message);
                    break;
                case DynamicItemStateMes.Types.DynamicItemStateType.Destroy:
                    OnDynamicDestroyMes(key, message);
                    break;
                default:
                    Console.WriteLine($"【场景动态物体中转】玩家{package.playerId}发送动态物体{message.DynamicItemId}状态{message.StateType}错误");
                    break;
            }
        }
        private void OnDynamicCreateMes((int, int) key, DynamicItemStateMes message)
        {
            if(message.ItemType == DynamicSceneItemType.ItemNone)
            {
                Console.WriteLine($"【场景动态物体中转】玩家{key.Item1}创建动态物体{key.Item2}类型错误");
                return;
            }
            if (_dynamicItemDic.ContainsKey(key))
            {
                Console.WriteLine($"【场景动态物体中转】玩家{key.Item1}重复创建动态物体{key.Item2}");
                return;
            }

            int dynamicItemId = _nextDynamicItemId++;
            _dynamicItemDic.Add(key, dynamicItemId);

            //DynamicItemStateMes mes = new()
            //{
            //    DynamicItemId = dynamicItemId,
            //    StateType = DynamicItemStateMes.Types.DynamicItemStateType.Create,
            //    ItemType = message.ItemType,
            //};
            message.DynamicItemId = dynamicItemId;
            message.CustomParams = message.CustomParams;

            foreach (var player in _playerSet)
            {
#if !LOCALDEBUG
                iif (player == key.Item1)
                    continue;
#endif
                SendTo(new(player, SetHeader(), message));
            }
            Console.WriteLine($"【场景动态物体中转】玩家{key.Item1}创建动态物体{key.Item2}类型{message.ItemType}，分配动态物体Id{dynamicItemId}");

            EventBus.Instance.Trigger(EventType.OnDynamicSceneItemAdd, new DynamicSceneItem(key.Item1, key.Item2, dynamicItemId, message.ItemType));
        }
        private void OnDynamicDestroyMes((int, int) key, DynamicItemStateMes message)
        {
            if (!_dynamicItemDic.ContainsKey(key))
            {
                Console.WriteLine($"【场景动态物体中转】玩家{key.Item1}销毁不存在的动态物体{key.Item2}");
                return;
            }
            int dynamicItemId = _dynamicItemDic[key];
            _dynamicItemDic.Remove(key);

            //DynamicItemStateMes mes = new()
            //{
            //    DynamicItemId = dynamicItemId,
            //    StateType = DynamicItemStateMes.Types.DynamicItemStateType.Destroy,
            //    ItemType = type
            //};
            message.DynamicItemId = dynamicItemId;
            message.CustomParams = message.CustomParams;

            foreach (var player in _playerSet)
            {
#if !LOCALDEBUG
                if (player == key.Item1)
                    continue;
#endif
                SendTo(new(player, SetHeader(), message));
            }

            EventBus.Instance.Trigger(EventType.OnDynamicSceneItemRemove, new DynamicSceneItem(key.Item1, key.Item2, dynamicItemId, message.ItemType));
        }
    }
}
