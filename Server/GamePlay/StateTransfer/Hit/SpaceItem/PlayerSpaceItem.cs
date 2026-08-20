using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.AttackRequest;
using Server.GamePlay.StateTransfer.Hit.TriggerItem;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerSpaceItem : SpaceItem,IOnShootHit,IOnExplosionHit,IOnAreaElementDamage,IOnHyperBloomHit,IOnTrigger
    {
        private const int WINDOW_SIZE = 150;
        public int playerId { get;private set;  }
        public DateTime preTime;
        public HistoryBuffer<AABB> history { get; private set; } = new(WINDOW_SIZE);
        public PlayerSpaceItem(int playerId) => this.playerId = playerId;

        public void OnShootHit(ShootHitReq req, List<int> sendList)
        {
            Vector3Message originMes = new();
            Vector3Message dirMes = new();

            originMes.Switch(req.ray.origin);
            dirMes.Switch(req.ray.dir);

            PlayerShootHitMessage hitMes = new()
            {
                Origin = originMes,
                Dir = dirMes,
                ElementAttack = req.elementAttack,
            };
            UdpHeader udpHeader = new();
            udpHeader.IsResponse = true;

            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, hitMes));
            Console.WriteLine($"【玩家空间物体】命中玩家{playerId}");
        }

        public void OnExplosionHit(IExplosionHitReq req)
        {
            PlayerExpHitMessage hitMes = new()
            {
                Center = new(),
                Radius = req.range.radius,
                ElementAttack = req.elementAttack,
            };
            hitMes.Center.Switch(req.range.center);

            UdpHeader udpHeader = new() { IsResponse = true };

            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, hitMes));
            Console.WriteLine($"【玩家空间物体】爆炸命中玩家{playerId}");
        }

        public float TryShootHit(ShootHitReq req)
        {
            if(!history.TryGetLerp(req.tick,out var bound,(a,b,num) => AABB.Lerp(a,b,num)))
                return -1;
            if (req.ray.IntersectAABB(bound, out var dis))
                return dis;
            return -1;
        }

        public bool TryExplosionHit(IExplosionHitReq req)
        {
            if(!history.TryGetLerp(req.tick, out var bound, (a, b, num) => AABB.Lerp(a, b, num)))
                return false;
            return SpaceUtility.IsIntersect(req.range, bound);
        }

        public bool TryAreaElementDamage(AreaElementDamageReq req)
        {
            if (!history.TryGetLerp(req.tick, out var bound, (a, b, num) => AABB.Lerp(a, b, num)))
                return false;
            return SpaceUtility.IsIntersect(req.area, bound);
        }
        public void OnAreaElementDamageHit(AreaElementDamageReq req)
        {
            UdpHeader udpHeader = new() { IsResponse = true };
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, req.damageMes));
            Console.WriteLine($"【玩家空间物体】范围元素伤害命中玩家{playerId}");
        }

        public float TryHyperBloomHit(HyperBloomHitReq req)
        {
            if (!history.TryGetLerp(req.tick, out var bound, (a, b, num) => AABB.Lerp(a, b, num)))
                return -1;
            if (req.range.IntersectAABB(bound))
                return Vector3.Distance(bound.center,req.range.center);
            return -1;
        }
        public void OnHyperBloomHit(HyperBloomHitReq req)
        {
            if(req.attackMessage == null)
                return;
            if (!history.TryGetLerp(req.tick, out var bound, (a, b, num) => AABB.Lerp(a, b, num)))
                return;
            PlayerShootHitMessage hitMes = new()
            {
                Origin = new(),
                Dir = new(),
                ElementAttack = req.attackMessage
            };
            hitMes.Origin.Switch(req.range.center);
            hitMes.Dir.Switch(Vector3.Normalize(bound.center - req.range.center));
            UdpHeader udpHeader = new() { IsResponse = true };
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, hitMes));
            Console.WriteLine($"【玩家空间物体】蔓生弹命中玩家{playerId}");
        }

        public float TryTrigger(TriggerItem triggerItem,long tick)
        {
            if (triggerItem is not ElementCrystalTriggerItem)
                return -1;
            if (!history.TryGetLerp(tick, out var bound, (a, b, num) => AABB.Lerp(a, b, num)))
                return -1;
            if (triggerItem.Bound.IsIntersect(bound))
                return Vector3.Distance(bound.center, triggerItem.Bound.center);
            return -1;
        }
        public void OnTrigger(TriggerItem triggerItem)
        {
            if (triggerItem is not ElementCrystalTriggerItem ecTriggerItem)
                return;
            PlayerElementShieldMessage message = new() { ElementType = ecTriggerItem.elementType };
            UdpHeader udpHeader = new() { IsResponse = true };
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, message));
            Console.WriteLine($"【玩家空间物体】玩家{playerId}触发元素护盾");
        }
    }
}
