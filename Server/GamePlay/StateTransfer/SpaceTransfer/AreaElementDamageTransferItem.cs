using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public struct AreaElementDamageReq
    {
        public int playerSpaceId;
        public long tick;
        public AreaElementDamageMes damageMes;
        public Sphere area;
    }
    public interface IOnAreaElementDamage
    {
        bool TryAreaElementDamage(AreaElementDamageReq areaElementDamageReq);
        void OnAreaElementDamageHit(AreaElementDamageReq areaElementDamageReq);
    }
    public class AreaElementDamageTransferItem : BaseSpaceTransferItem
    {
        private PriorityQueue<AreaElementDamageReq, long> _areaHitReqQueue = new();
        private object _areaHitReqLock = new();
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
                || areaMes.ElementAttack == null)
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
                    playerSpaceId = playerSpace.spaceId,
                    tick = udpHeader.Time,
                    damageMes = areaMes,
                    area = range,
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
            var maskId = req.playerSpaceId;
            var hitSpaceItem = spaceTree.SphereOverlap(req.area, maskId);

            foreach (var item in hitSpaceItem)
                if (item is IOnAreaElementDamage elementDamage)
                    if (elementDamage.TryAreaElementDamage(req))
                        elementDamage.OnAreaElementDamageHit(req);
        }
    }
}
