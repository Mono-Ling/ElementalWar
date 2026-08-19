using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.AttackRequest;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Server.Message.Tools;

namespace Server.GamePlay.StateTransfer
{
    public class DynamicSpaceItem : SpaceItem,IOnShootHit,IOnExplosionHit,IOnAreaElementDamage
    {
        public bool IsEnable => _isEnable;
        private bool _isEnable;
        public DynamicSceneItem dynamicItem;
        public DynamicSpaceItem(DynamicSceneItem item)
        {
            this.dynamicItem = item;
            if(item.customParams == null)
            {
                Debug.LogError("【动态空间物体】包围盒初始化信息为空");
                return;
            }
            if(!item.customParams.TryUnpack<BoundStateMessage>(out var boundMes))
            {
                Debug.LogError("【动态空间物体】包围盒初始化信息不存在");
                return;
            }
            (var center, _) = boundMes.Center;
            (var extents,_) = boundMes.Extents;
            this.bound = new(center,extents);
            _isEnable = true;
        }

        public void OnAreaElementDamageHit(AreaElementDamageReq req)
            => OnAttack(req.fromPlayerId, req.damageMes.ElementAttack.ToArray());
        public void OnExplosionHit(IExplosionHitReq req)
            => OnAttack(req.fromPlayerId, req.elementAttack);
        public void OnShootHit(ShootHitReq req, List<int> sendList)
            => OnAttack(req.fromPlayerId, req.elementAttack);
        public bool TryAreaElementDamage(AreaElementDamageReq req)
            => req.area.IntersectAABB(bound);
        public bool TryExplosionHit(IExplosionHitReq req)
            => req.range.IntersectAABB(bound);

        public float TryShootHit(ShootHitReq req)
        {
            if (req.ray.IntersectAABB(bound, out var dis))
                return dis;
            return -1;
        }
        private void OnAttack(int fromPlayerId,params ElementAttackMessage[] elementAttacks)
        {
            if (elementAttacks == null || elementAttacks.Length == 0)
                return;
            DynamicItemHitMessage hitMes = new()
            {
                ClientDynamicItemId = dynamicItem.clientDynamicItemId,
                FromPlayerId = fromPlayerId,
            };
            foreach(var attack in elementAttacks)
                hitMes.ElementAttack.Add(attack);
            UdpHeader udpHeader = new() { IsResponse = true };
            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(dynamicItem.playerId, udpHeader, hitMes));
        }
    }
}
