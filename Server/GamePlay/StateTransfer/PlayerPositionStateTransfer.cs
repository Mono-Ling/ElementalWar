using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Message;
using Server.Event;
using Server.Message.Tools;
using Server.Scene;
using Server.Scene.Scripts;

namespace Server.GamePlay.StateTransfer
{
    public class PlayerPositionState
    {
        public Vector3 pos = new(3, 0, 3);
        public Quaternion rot;
        public float pitch;

        public DateTime preTime;
    }
    public class PlayerPositionStateTransfer : BaseTransfer
    {
        public const float THRESHOLD = 0.5f;
        private const string BIRTH_POINT_PATH = @"D:\Unity\Project\ElementalWar\Server\Scene\BirthPoint.json";
        private Dictionary<int, PlayerPositionState> _posStateDic = new();
        public override void Init(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            foreach (int id in playerList)
            {
                if (_posStateDic.ContainsKey(id))
                {
                    Console.WriteLine($"【玩家位置状态中转】玩家{id}重复注册");
                    continue;
                }
                _posStateDic.Add(id, new());
            }
            InitPosition(_posStateDic.Values.ToArray());
        }
        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            AddListener<PositionStateMessage>(OnPositionStateSyn);
        }
        public override void Update()
        {
            PlayerPosStateMesMap message = new();
            message.ServerTime = DateTime.UtcNow.Ticks;
            // 收集所有玩家的当前状态快照
            foreach (var item in _posStateDic)
            {
                var pos = item.Value.pos;
                var rot = item.Value.rot;
                Vector3Message posMes = new() { X = pos.X, Y = pos.Y, Z = pos.Z };
                QuaternionMessage rotMes = new() { X = rot.X, Y = rot.Y, Z = rot.Z, W = rot.W };
                PositionStateMessage stateMes = new() { Pos = posMes, Rot = rotMes, Pitch = item.Value.pitch };
                message.PlayerPosStateMap.Add(item.Key, stateMes);
            }

            foreach (var item in _posStateDic)
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(item.Key, SetHeader(false), message));
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
                (var pos, _) = posMes.Pos;
                if ((pos - posState.pos).Length() > THRESHOLD)
                    return;
                posState.pos = new(posMes.Pos.X,posMes.Pos.Y,posMes.Pos.Z);
                posState.rot = new(posMes.Rot.X, posMes.Rot.Y, posMes.Rot.Z, posMes.Rot.W);
                posState.pitch = posMes.Pitch;

                posState.preTime = clientTIme;
            }
            else
                Console.WriteLine($"【玩家位置状态中转】玩家{package.playerId}不存在");
        }
        private void InitPosition(PlayerPositionState[] posStates)
        {
            if(posStates.Length == 0)
                return;
            try
            {
                var json = File.ReadAllText(BIRTH_POINT_PATH);
                var info = JsonSerializer.Deserialize<BirthPointInfo>(json);

                if (info == null || info.positions == null || info.positions.Count == 0)
                {
                    Debug.LogError("【玩家位置状态中转】出生点加载失败");
                    return;
                }

                int length = info.positions.Count;
                for (int i = 0; i < posStates.Length; i++)
                {
                    int j = i % length;
                    posStates[i].pos = info.positions[j].Switch();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("【玩家位置状态中转】位置初始化异常" + ex.Message);
            }
        }
    }
}
