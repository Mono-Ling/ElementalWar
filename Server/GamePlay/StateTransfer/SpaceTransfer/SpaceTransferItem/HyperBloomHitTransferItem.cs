
using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.AttackRequest;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public interface IOnHyperBloomHit
    {
        float TryHyperBloomHit(HyperBloomHitReq req);
        void OnHyperBloomHit(HyperBloomHitReq req);
    }
    public class HyperBloomHitTransferItem : BaseSpaceTransferItem
    {
        private PriorityQueue<HyperBloomHitReq, long> _hyperBloomHitReqQueue = new();
        private object _hyperBloomHitReqLock = new();

        private HashSet<DynamicSceneItem> _grassCoreSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicItemAdd);
            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicRemove);
            AddListener<HyperBloomRequestMessage>(OnHyperBloomRequest);
        }
        public override void Update()
        {
            while (_hyperBloomHitReqQueue.TryGet(out var req, _hyperBloomHitReqLock))
                HyperBloomHitCheck(req);
        }
        public override void Stop()
        {
            RemoveListener<HyperBloomRequestMessage>(OnHyperBloomRequest);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicItemAdd);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemRemove, OnDynamicRemove);
        }
        private void OnDynamicItemAdd(DynamicSceneItem item)
        {
            if (item.itemType != DynamicSceneItemType.GrassCore || _grassCoreSet.Contains(item))
                return;
            _grassCoreSet.Add(item);
        }
        private void OnDynamicRemove(DynamicSceneItem item)
        {
            if (item.itemType != DynamicSceneItemType.GrassCore || !_grassCoreSet.Contains(item))
                return;
            _grassCoreSet.Remove(item);
        }
        private void OnHyperBloomRequest(ClientPackage package)
        {
            if (package.message is not HyperBloomRequestMessage message)
                return;
            if (package.header is not UdpHeader header)
                return;
            DynamicSceneItem item = new(package.playerId, message.ClientDynamicItemId,default, DynamicSceneItemType.GrassCore);
            if (!_grassCoreSet.Contains(item))
                return;
            (var center,_) = message.Center;
            Sphere range = new(center,message.Radius);
            HyperBloomHitReq req = new(message.MaskPlayerId,header.Time, range, message.ElementAttack);
            lock (_hyperBloomHitReqLock)
                _hyperBloomHitReqQueue.Enqueue(req,header.Time);
        }
        private void HyperBloomHitCheck(HyperBloomHitReq req)
        {
            int maskId = -1;
            if (req.maskPlayerId >= 0 && (playerSpaceItemDic?.TryGetValue(req.maskPlayerId, out var spaceItem) ?? false))
                maskId = spaceItem.spaceId;
            var hitItems = spaceTree.SphereOverlap(req.range, maskId);

            float minDis = float.MaxValue;
            IOnHyperBloomHit? hitItem = null;
            foreach(var hit in hitItems)
                if(hit != null && hit is IOnHyperBloomHit bloomHit)
                {
                    var dis = bloomHit.TryHyperBloomHit(req);
                    if(dis < 0) continue;
                    if(dis < minDis)
                    {
                        minDis = dis;
                        hitItem = bloomHit;
                    }
                }

            hitItem?.OnHyperBloomHit(req);
        }
    }
}
