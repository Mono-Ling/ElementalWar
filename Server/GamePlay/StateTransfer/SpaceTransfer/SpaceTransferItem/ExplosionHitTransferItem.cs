using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public struct ExplosionHitReq
    {
        public DynamicSceneItem dynamicItem;
        public ElementAttackMessage elementAttack;
        public long tick;
        public Sphere range;
        public ExplosionHitReq(DynamicSceneItem item, ElementAttackMessage elementAttack, Sphere range,long tick)
        {
            this.dynamicItem = item;
            this.range = range;
            this.elementAttack = elementAttack;
            this.tick = tick;
        }
    }
    public interface IOnExplosionHit
    {
        bool TryExplosionHit(ExplosionHitReq req);
        void OnExplosionHit(ExplosionHitReq req);
    }
    public class ExplosionHitTransferItem : BaseSpaceTransferItem
    {
        private PriorityQueue<ExplosionHitReq, long> _explosionHitReqQueue = new();
        private object _explosionHitReqLock = new();

        private HashSet<DynamicSceneItem> _grenadeItemSet = new();

        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);

            EventBus.Instance.AddListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicAdd);
            AddListener<ExplosionRequestMessage>(OnExpReqReceive);
        }
        public override void Update()
        {
            while(_explosionHitReqQueue.TryGet(out var req,_explosionHitReqLock))
                OnExpHitCheck(req);
        }
        public override void Stop()
        {
            RemoveListener<ExplosionRequestMessage>(OnExpReqReceive);
            EventBus.Instance.RemoveListener<DynamicSceneItem>(EventType.OnDynamicSceneItemAdd, OnDynamicAdd);
        }
        private void OnDynamicAdd(DynamicSceneItem item)
        {
            if (item.itemType == DynamicSceneItemType.Grenade)
            {
                _grenadeItemSet.Add(item);
            }
        }
        private void OnExpReqReceive(ClientPackage package)
        {
            if (package.message is not ExplosionRequestMessage reqMes
                || reqMes.ElementAttack == null)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;
            DynamicSceneItem item = new(package.playerId, reqMes.ClientDynamicItemId, default, DynamicSceneItemType.Grenade);
            if (!_grenadeItemSet.TryGetValue(item, out var foundItem))
                return;
            var (center, _) = reqMes.Center;
            var radius = reqMes.Radius;
            Sphere range = new(center, radius);
            ExplosionHitReq req = new(foundItem,reqMes.ElementAttack, range,udpHeader.Time);
            lock (_explosionHitReqLock)
                _explosionHitReqQueue.Enqueue(req, udpHeader.Time);
        }
        private void OnExpHitCheck(ExplosionHitReq req)
        {
            if(spaceTree == null)
            {
                Debug.LogError("【爆炸命中检测中转】空间树未初始化");
                return;
            }
            if(!_grenadeItemSet.TryGetValue(req.dynamicItem,out var foundItem))
                return;
            _grenadeItemSet.Remove(foundItem);

            var hitSpaceItem = spaceTree.SphereOverlap(req.range);

            foreach (var item in hitSpaceItem)
                if (item is IOnExplosionHit hitItem)
                    if(hitItem.TryExplosionHit(req))
                        hitItem.OnExplosionHit(req);
        }
    }
}
