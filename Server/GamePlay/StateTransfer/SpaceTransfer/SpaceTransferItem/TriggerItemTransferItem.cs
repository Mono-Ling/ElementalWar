using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.StateTransfer.Hit.TriggerItem;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public interface IOnTrigger
    {
        float TryTrigger(TriggerItem triggerItem);
        void OnTrigger(TriggerItem triggerItem);
    }
    public class TriggerItemTransferItem : BaseSpaceTransferItem
    {
        private Dictionary<DynamicSceneItem, TriggerItem> _triggerItemDic = new();
        private ConcurrentQueue<DynamicSceneItem> _addQueue = new();
        private ConcurrentQueue<DynamicSceneItem> _removeQueue = new();
        private HashSet<DynamicSceneItemType> _listenItemDic = new();
        public TriggerItemTransferItem() : this(DynamicSceneItemType.ElementCrystal) { }
        public TriggerItemTransferItem(params DynamicSceneItemType[] types)
        {
            foreach (var type in types)
                _listenItemDic.Add(type);
        }
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicItemAdd);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicItemRemove);
        }
        public override void Update()
        {
            while (_addQueue.TryDequeue(out var item))
                AddTriggerItem(item);
            while (_removeQueue.TryDequeue(out var item))
                RemoveTriggerItem(item);

            UpdateTrigger();
        }
        public override void Stop()
        {
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicItemAdd);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicItemRemove);
        }
        private void AddTriggerItem(DynamicSceneItem item)
        {
            if ((!playerSpaceItemDic?.ContainsKey(item.playerId) ?? true) ||
                !_listenItemDic.Contains(item.itemType) ||
                _triggerItemDic.ContainsKey(item))
                return;
            ITriggerItemInitMessage? message = null;
            TriggerItem.GetTriggerItemInitMessage(item.itemType,item.customParams, ref message);
            if (message == null)
                return;
            var triggerItem = TriggerItem.GetTriggerItem(item.itemType);
            if(triggerItem == null)
                return;
            if (!triggerItem.InitTriggerItem(item, message))
                return;
            _triggerItemDic.Add(item,triggerItem);
        }
        private void RemoveTriggerItem(DynamicSceneItem item)
        {
            if (!_triggerItemDic.ContainsKey(item))
                return;
            _triggerItemDic.Remove(item);
        }
        private void UpdateTrigger()
        {
            foreach(var triggerItem in _triggerItemDic.Values)
            {
                if(triggerItem == null || !triggerItem.IsEnable)
                    continue;
                var triggerSpaceItems = spaceTree?.BoxOverlap(triggerItem.Bound) ?? new();
                float minDis = float.MaxValue;
                IOnTrigger? triggerSpaceItem = null;

                foreach(var spaceItem in triggerSpaceItems)
                {
                    if(spaceItem is IOnTrigger onTrigger)
                    {
                        var dis = onTrigger.TryTrigger(triggerItem);
                        if (dis < 0)
                            continue;
                        if(dis < minDis)
                        {
                            minDis = dis;
                            triggerSpaceItem = onTrigger;
                        }
                    }
                }

                if (triggerSpaceItem == null)
                    continue;
                triggerSpaceItem?.OnTrigger(triggerItem);
                triggerItem.OnTrigger();
            }
        }
        private void OnDynamicItemAdd(DynamicSceneItem item)
            => _addQueue.Enqueue(item);
        private void OnDynamicItemRemove(DynamicSceneItem item)
            => _removeQueue.Enqueue(item);
    }
}
