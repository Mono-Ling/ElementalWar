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
        public int maskSpaceId;
        public Ray ray;
        public ShootHitReq(int playerId, Ray ray, int maskSpaceId)
        {
            this.playerId = playerId;
            this.ray = ray;
            this.maskSpaceId = maskSpaceId;
        }
    }
    public interface IOnShootHit
    {
        void OnShootHit(Ray ray, List<int> sendList);
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

            while (TryGetShootHitReq(out var shootHitReq))
                OnShootHitCheck(shootHitReq);
        }
        public override void Stop()
            => RemoveListener<ShootRequestMessage>(OnShootReqReceive);
        private void OnShootReqReceive(ClientPackage package)
        {
            if (package.message == null || package.message is not ShootRequestMessage boundMes)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;

            // 过滤无效玩家
            if (playerSpaceItemDic.TryGetValue(package.playerId, out var playerSpace))
            {
                var (origin, _) = boundMes.Origin;
                var (dir, _) = boundMes.Dir;
                Ray ray = new(origin, dir);
                ShootHitReq req = new(package.playerId, ray, playerSpace.spaceId);

                lock (_shootHitReqLock)
                    _shootHitReqQueue.Enqueue(req, udpHeader.Time);
                Console.WriteLine("【命中检测中转】射击请求");
            }
            else
                Console.WriteLine($"【命中检测中转】玩家{package.playerId}不存在，无效射击判定请求");
        }
        private bool TryGetShootHitReq(out ShootHitReq req)
        {
            lock (_shootHitReqLock)
            {
                if (_shootHitReqQueue.Count == 0)
                {
                    req = default;
                    return false;
                }
                return _shootHitReqQueue.TryDequeue(out req, out _);
            }
        }
        private void OnShootHitCheck(ShootHitReq req)
        {
            if(spaceTree == null)
            {
                Console.WriteLine("【命中检测中转】空间树未初始化");
                return;
            }
            // 启用时需将mask设为玩家spaceId
            int mask = -1;
            mask = req.maskSpaceId;
            if (spaceTree.RayCast(req.ray, out var hit, out _, mask))
            {
                if (hit != null && hit is IOnShootHit onHit)
                    onHit.OnShootHit(req.ray, _sendList);
            }
        }
    }
}
