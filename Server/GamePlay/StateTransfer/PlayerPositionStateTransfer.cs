using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerPositionState
    {
        public Vector3 pos;
        public Quaternion rot;
        public float pitch;

        public DateTime preTime;
    }
    public class PlayerPositionStateTransfer : BaseTransfer
    {
        private Dictionary<int, PlayerPositionState> _posStateDic = new();
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach(int id in  playerList)
            {
                if (_posStateDic.ContainsKey(id))
                {
                    Console.WriteLine($"【玩家位置状态中转】玩家{id}重复注册");
                    continue;
                }
                _posStateDic.Add(id, new());
            }
            AddListener<PositionStateMessage>(OnPositionStateSyn);
        }
        public override void Update()
        {
            // 先收集所有玩家的当前状态快照
            Dictionary<int, PositionStateMessage> allStates = new();
            DateTime serverTime = DateTime.UtcNow;
            foreach (var item in _posStateDic)
            {
                var pos = item.Value.pos;
                var rot = item.Value.rot;
                Vector3Message posMes = new() { X = pos.X, Y = pos.Y, Z = pos.Z };
                QuaternionMessage rotMes = new() { X = rot.X, Y = rot.Y, Z = rot.Z, W = rot.W };
                PositionStateMessage stateMes = new() { Pos = posMes, Rot = rotMes, Pitch = item.Value.pitch };
                allStates[item.Key] = stateMes;
            }

            // 逐玩家广播独立 map 实例，避免引用竞态（多个 ClientPackage 共享同一 mesMap 引用）
            foreach (var item in _posStateDic)
            {
                PlayerPosStateMesMap mesMap = new();
                mesMap.ServerTime = serverTime.Ticks;
                mesMap.ClientId = item.Key;
                foreach (var kv in allStates)
                    mesMap.PlayerPosStateMap.Add(kv.Key, kv.Value);

                udpHeader = new();
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(item.Key, udpHeader, mesMap));
            }
        }
        public override void Stop()
            => RemoveListener<PositionStateMessage>(OnPositionStateSyn);
        private void OnPositionStateSyn(ClientPackage package)
        {
            if (package.message == null || package.message is not PositionStateMessage posMes)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;
            DateTime clientTIme = new(udpHeader.Time);
            if(_posStateDic.TryGetValue(package.playerId,out var posState))
            {
                if (clientTIme < posState.preTime)
                    return;
                posState.pos = new(posMes.Pos.X,posMes.Pos.Y,posMes.Pos.Z);
                posState.rot = new(posMes.Rot.X, posMes.Rot.Y, posMes.Rot.Z, posMes.Rot.W);
                posState.pitch = posMes.Pitch;

                posState.preTime = clientTIme;
            }
            else
                Console.WriteLine($"【玩家位置状态中转】玩家{package.playerId}不存在");
        }
    }
}
