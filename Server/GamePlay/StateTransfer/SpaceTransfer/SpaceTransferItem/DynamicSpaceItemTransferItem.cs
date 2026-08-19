using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public class DynamicSpaceItemTransferItem : BaseSpaceTransferItem
    {
        private HashSet<DynamicSceneItemType> _listenItemTypes = new();
        private Dictionary<DynamicSceneItem,DynamicSpaceItem> _dynamicItemDic = new();
        private ConcurrentQueue<DynamicSpaceItem> _addQueue = new();
        private ConcurrentQueue<DynamicSceneItem> _removeQueue = new();
        public DynamicSpaceItemTransferItem() : this
            (
                DynamicSceneItemType.GrassCore
            ) { }
        public DynamicSpaceItemTransferItem(params DynamicSceneItemType[] itemTypes)
        {
            foreach(var type in itemTypes)
                _listenItemTypes.Add(type);
        }
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicSpaceItemAdd);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicItemRemove);
        }
        public override void Update()
        {
            while (_addQueue.TryDequeue(out var item))
            {
                spaceTree?.Add(item);
                _dynamicItemDic.Add(item.dynamicItem,item);
            }

            while(_removeQueue.TryDequeue(out var item))
                if(_dynamicItemDic.TryGetValue(item,out var spaceItem))
                {
                    spaceTree?.Remove(spaceItem);
                    _dynamicItemDic.Remove(item);
                }
        }
        public override void Stop()
        {
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicSpaceItemAdd);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicItemRemove);
        }
        private void OnDynamicSpaceItemAdd(DynamicSceneItem item)
        {
            if (!_listenItemTypes.Contains(item.itemType) ||
                (!playerSpaceItemDic?.ContainsKey(item.playerId) ?? true) ||
                _dynamicItemDic.ContainsKey(item))
                return;
            DynamicSpaceItem spaceItem = new(item);
            if(!spaceItem.IsEnable)
                return;
            _addQueue.Enqueue(spaceItem);
        }
        private void OnDynamicItemRemove(DynamicSceneItem item)
        {
            if (!_listenItemTypes.Contains(item.itemType) ||
                (!playerSpaceItemDic?.ContainsKey(item.playerId) ?? true) ||
                !_dynamicItemDic.ContainsKey(item))
                return;
            _removeQueue.Enqueue(item);
        }
    }
}
