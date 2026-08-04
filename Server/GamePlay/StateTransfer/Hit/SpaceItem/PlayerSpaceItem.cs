using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Server.Message.Tools;
using Space;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerSpaceItem : SpaceItem,IOnShootHit,IOnExplosionHit
    {
        public int playerId { get;private set;  }
        public DateTime preTime;
        public PlayerSpaceItem(int playerId) => this.playerId = playerId;

        public void OnShootHit(Ray ray, List<int> sendList)
        {
            Vector3Message originMes = new();
            Vector3Message dirMes = new();

            originMes.Switch(ray.origin);
            dirMes.Switch(ray.dir);

            PlayerShootHitMessage hitMes = new() { Origin = originMes, Dir = dirMes };
            UdpHeader udpHeader = new();
            udpHeader.IsResponse = true;

            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, hitMes));
            Console.WriteLine($"【玩家空间物体】命中玩家{playerId}");
        }

        public void OnExplosionHit(Sphere range, List<int> sendList)
        {
            PlayerExpHitMessage hitMes = new() { Center = new(), Radius = range.radius };
            hitMes.Center.Switch(range.center);

            UdpHeader udpHeader = new() { IsResponse = true };

            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, udpHeader, hitMes));
            Console.WriteLine($"【玩家空间物体】爆炸命中玩家{playerId}");
        }
    }
}
