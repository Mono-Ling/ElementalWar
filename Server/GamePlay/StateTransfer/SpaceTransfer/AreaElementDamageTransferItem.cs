using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.GamePlay.AttackRequest;
using Server.Message.Tools;
using Space;
using AOEType = Message.AreaElementDamageMes.Types.AOEType;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public interface IOnAreaElementDamage
    {
        bool TryAreaElementDamage(AreaElementDamageReq areaElementDamageReq);
        void OnAreaElementDamageHit(AreaElementDamageReq areaElementDamageReq);
    }
    public class AreaElementDamageTransferItem : BaseSpaceTransferItem
    {
        private PriorityQueue<AreaElementDamageReq, long> _areaHitReqQueue = new();
        private object _areaHitReqLock = new();

        private HashSet<int> _maskSet = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            AddListener<AreaElementDamageMes>(OnAreaElementDamageReceive);
        }
        public override void Update()
        {
            while (_areaHitReqQueue.TryGet(out var areaHitReq, _areaHitReqLock))
                OnAreaDamageCheck(areaHitReq);
        }
        public override void Stop()
            => RemoveListener<AreaElementDamageMes>(OnAreaElementDamageReceive);
        private void OnAreaElementDamageReceive(ClientPackage package)
        {
            if (package.message is not AreaElementDamageMes areaMes
                || areaMes.ElementAttack.Count == 0)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;
            if (playerSpaceItemDic.TryGetValue(package.playerId, out var playerSpace))
            {
                (var center, _) = areaMes.Center;
                var radius = areaMes.Radius;
                Sphere range = new(center, radius);
                AreaElementDamageReq req = new()
                {
                    fromPlayerId = areaMes.ElementAttack[0].FromPlayerId,
                    maskSpaceId = playerSpace.spaceId,
                    tick = udpHeader.Time,
                    damageMes = areaMes,
                    area = range,
                    aoeType = areaMes.AoeType,
                };

                lock (_areaHitReqLock)
                    _areaHitReqQueue.Enqueue(req, udpHeader.Time);
            }
        }
        private void OnAreaDamageCheck(AreaElementDamageReq req)
        {
            if (spaceTree == null)
            {
                Debug.LogError("【元素范围伤害命中检测中转】空间树未初始化");
                return;
            }

            List<SpaceItem>? hitSpaceItem = null;

            switch (req.aoeType)
            {
                case AOEType.Normal:
                    _maskSet.Clear();
                    var maskId = req.maskSpaceId;
                    _maskSet.Add(maskId);
                    if (req.fromPlayerId >= 0 && playerSpaceItemDic.TryGetValue(req.fromPlayerId, out var spaceItem))
                        _maskSet.Add(spaceItem.spaceId);

                    hitSpaceItem = spaceTree.SphereOverlapMask(req.area, _maskSet);
                    foreach (var item in hitSpaceItem)
                        if (item is IOnAreaElementDamage normalHitItem)
                            if (normalHitItem.TryAreaElementDamage(req))
                                normalHitItem.OnAreaElementDamageHit(req);
                    
                    break;
                case AOEType.Explosion:
                    hitSpaceItem = spaceTree.SphereOverlap(req.area);
                    var attacks = req.damageMes.ElementAttack;
                    if (attacks.Count == 0)
                        break;
                    PlayerExpHitReq expReq = new()
                    {
                        fromPlayerId = req.fromPlayerId,
                        elementAttack = attacks[0],
                        tick = req.tick,
                        range = req.area,
                    };
                    foreach(var item in hitSpaceItem)
                        if (item is IOnExplosionHit expHitItem)
                            if (expHitItem.TryExplosionHit(expReq))
                                expHitItem.OnExplosionHit(expReq);
                    break;
                default:
                    break;
            }
        }
    }
}
