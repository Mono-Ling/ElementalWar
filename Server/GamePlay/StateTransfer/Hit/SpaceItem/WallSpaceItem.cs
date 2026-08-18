using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.AttackRequest;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer
{
    public class WallSpaceItem : SpaceItem,IOnShootHit
    {
        public int wallId {  get; private set; }
        public WallSpaceItem(int wallId,AABB bound)
        {
            this.wallId = wallId;
            this.bound = bound;
        }

        public void OnShootHit(ShootHitReq req, List<int> sendList)
        {
            Vector3Message originMes = new();
            Vector3Message dirMes = new();

            originMes.Switch(req.ray.origin);
            dirMes.Switch(req.ray.dir);

            WallShootHitMessage hitMes = new()
            {
                WallId = wallId,
                Origin = originMes,
                Dir = dirMes,
                ElementAttack = req.elementAttack,
            };
            // 后期可优化为AOI控制流量
            foreach (int player in sendList)
            {
                UdpHeader udpHeader = new();
                udpHeader.IsResponse = true;
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(player, udpHeader, hitMes));
            }
            Console.WriteLine($"【围墙空间物体】命中Wall{wallId}");
        }

        public float TryShootHit(ShootHitReq req)
        {
            if (req.ray.IntersectAABB(bound, out var dis))
                return dis;
            return -1;
        }
    }
}
