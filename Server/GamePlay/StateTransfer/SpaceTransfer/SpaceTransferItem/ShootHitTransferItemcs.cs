using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public struct ShootHitReq
    {
        public int playerId;
        public ElementAttackMessage elementAttack;
        public long tick;
        public int maskSpaceId;
        public Ray ray;
        public ShootHitReq(int playerId, Ray ray, ElementAttackMessage elementAttack, int maskSpaceId,long tick)
        {
            this.playerId = playerId;
            this.elementAttack = elementAttack;
            this.ray = ray;
            this.maskSpaceId = maskSpaceId;
            this.tick = tick;
        }
    }
    public interface IOnShootHit
    {
        float TryShootHit(ShootHitReq req);
        void OnShootHit(ShootHitReq req, List<int> sendList);
    }
    public class ShootHitTransferItemcs : BaseSpaceTransferItem
    {
        private PriorityQueue<ShootHitReq, long> _shootHitReqQueue = new();
        private object _shootHitReqLock = new();
        private List<int> _sendList = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            _sendList = playerList;
            AddListener<ShootRequestMessage>(OnShootReqReceive);
        }
        public override void Update()
        {

            while (_shootHitReqQueue.TryGet(out var shootHitReq,_shootHitReqLock))
                OnShootHitCheck(shootHitReq);
        }
        public override void Stop()
            => RemoveListener<ShootRequestMessage>(OnShootReqReceive);
        private void OnShootReqReceive(ClientPackage package)
        {
            if (package.message == null
                || package.message is not ShootRequestMessage reqMes
                || reqMes.ElementAttack == null)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;

            // 过滤无效玩家
            if (playerSpaceItemDic.TryGetValue(package.playerId, out var playerSpace))
            {
                var (origin, _) = reqMes.Origin;
                var (dir, _) = reqMes.Dir;
                Ray ray = new(origin, dir);
                ShootHitReq req = new(package.playerId, ray,reqMes.ElementAttack, playerSpace.spaceId,udpHeader.Time);

                lock (_shootHitReqLock)
                    _shootHitReqQueue.Enqueue(req, udpHeader.Time);
                // Console.WriteLine("【命中检测中转】射击请求");
            }
            else
                Debug.LogWarning($"【命中检测中转】玩家{package.playerId}不存在，无效射击判定请求");
        }
        private void OnShootHitCheck(ShootHitReq req)
        {
            if(spaceTree == null)
            {
                Debug.LogError("【命中检测中转】空间树未初始化");
                return;
            }
            // 启用时需将mask设为玩家spaceId
            int mask = -1;
            mask = req.maskSpaceId;

            var hitList = spaceTree.RayOverlap(req.ray, mask);
            float minDis = float.MaxValue;
            IOnShootHit minHitItem = null;
            foreach ( var hit in hitList )
            {
                if (hit is not IOnShootHit hitItem)
                    continue;
                var dis = hitItem.TryShootHit(req);
                if(dis > 0 && dis < minDis)
                {
                    minDis = dis;
                    minHitItem = hitItem;
                }
            }

            minHitItem?.OnShootHit(req, _sendList);
        }
    }
}
