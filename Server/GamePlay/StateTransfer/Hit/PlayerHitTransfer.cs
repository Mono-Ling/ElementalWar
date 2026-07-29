using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Message;
using Space;
using Server.Message.Tools;

namespace Server.GamePlay.StateTransfer
{
    public struct ShootHitReq
    {
        public int playerId;
        public int maskSpaceId;
        public Ray ray;
        public ShootHitReq(int playerId, Ray ray,int maskSpaceId)
        {
            this.playerId = playerId;
            this.ray = ray;
            this.maskSpaceId = maskSpaceId;
        }
    }
    public class PlayerHitTransfer : BaseTransfer
    {
        private const float SCENE_CENTER_OFFSET = 10;
        private const float SCENE_X = 100;
        private const float SCENE_Y = 20;
        private const float SCENE_Z = 100;
        private Dictionary<int, PlayerSpaceItem> _playerSpaceItemDic = new();
        private SpaceTree _spaceTree = new(new AABB(Vector3.UnitY * SCENE_CENTER_OFFSET,new(SCENE_X,SCENE_Y,SCENE_Z)));

        private ConcurrentQueue<PlayerSpaceItem> _playerBoundUpdateQueue = new();
        private PriorityQueue<ShootHitReq, long> _shootHitReqQueue = new();
        private object _shootHitReqLock = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach(int player in playerList)
            {
                if (_playerSpaceItemDic.ContainsKey(player))
                {
                    Console.WriteLine($"【命中检测中转】玩家{player}重复注册");
                    continue;
                }
                PlayerSpaceItem spaceItem = new(player);
                _playerSpaceItemDic.Add(player, spaceItem);
                _spaceTree.Add(spaceItem);
            }
            AddListener<BoundStateMessage>(OnPlayerBoundStateSyn);
            AddListener<ShootRequestMessage>(OnShootReqReceive);
        }
        public override void Update()
        {
            while(_playerBoundUpdateQueue.TryDequeue(out var playerSpace))
                _spaceTree.UpdateItem(playerSpace);

            while(TryGetShootHitReq(out var shootHitReq))
                OnShootHitCheck(shootHitReq);
        }
        public override void Stop()
        {
            RemoveListener<BoundStateMessage>(OnPlayerBoundStateSyn);
            RemoveListener<ShootRequestMessage>(OnShootReqReceive);
        }
        private void OnPlayerBoundStateSyn(ClientPackage package)
        {
            if (package.message == null || package.message is not BoundStateMessage boundMes)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;
            DateTime clientTime = new(udpHeader.Time);
            if(_playerSpaceItemDic.TryGetValue(package.playerId,out var playerSpace))
            {
                if (playerSpace.preTime > clientTime)
                    return;
                (Vector3 center, _) = boundMes.Center;
                (Vector3 extents,_) = boundMes.Extents;

                playerSpace.bound = new(center, extents);

                _playerBoundUpdateQueue.Enqueue(playerSpace);

                playerSpace.preTime = clientTime;
            }
            else
                Console.WriteLine($"【【命中检测中转】玩家{package.playerId}不存在");
        }
        private void OnShootReqReceive(ClientPackage package)
        {
            if (package.message == null || package.message is not ShootRequestMessage boundMes)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;

            // 过滤无效玩家
            if (_playerSpaceItemDic.TryGetValue(package.playerId,out var playerSpace))
            {
                var (origin, _) = boundMes.Origin;
                var (dir, _) = boundMes.Dir;
                Ray ray = new(origin, dir);
                ShootHitReq req = new(package.playerId, ray,playerSpace.spaceId);

                lock (_shootHitReqLock)
                    _shootHitReqQueue.Enqueue(req, udpHeader.Time);
                Console.WriteLine("【命中检测中转】射击请求");
            }
            else
                Console.WriteLine($"【命中检测中转】玩家{package.playerId}不存在，无效射击判定请求");
        }
        private bool TryGetShootHitReq(out ShootHitReq req)
        {
            lock( _shootHitReqLock)
            {
                if(_shootHitReqQueue.Count == 0)
                {
                    req = default;
                    return false;
                }
                return _shootHitReqQueue.TryDequeue(out req, out _);
            }
        }
        private void OnShootHitCheck(ShootHitReq req)
        {
            // 启用时需将mask设为玩家spaceId
            int mask = -1;
            // mask = req.maskSpaceId;
            if (_spaceTree.RayCast(req.ray, out var hit, out _,mask))
            {
                if (hit is PlayerSpaceItem playerSpace)
                    OnHitPlayer(playerSpace.playerId, req.ray);
            }
        }
        private void OnHitPlayer(int playerId,Ray ray)
        {
            Vector3Message originMes = new();
            Vector3Message dirMes = new();

            originMes.Switch(ray.origin);
            dirMes.Switch(ray.dir);

            PlayerShootHitMessage hitMes = new() { Origin = originMes ,Dir = dirMes};
            SendTo(new(playerId, SetHeader(), hitMes));
            Console.WriteLine($"【命中检测中转】命中玩家{playerId}");
        }
    }
}
