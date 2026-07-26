using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Message;
using Server.Event;
using Server.GamePlay;

namespace Server.Test
{
    public class RemoteTest : IDisposable
    {
        private readonly Dictionary<int, PlayerStateTransfer> _transferDic = new();
        public RemoteTest()
        {
            EventBus.Instance.AddListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.AddListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
        }
        private void OnPlayerConnect(int playerId)
        {
            // 以防万一清理旧的（同一 id 重复连接）
            if (_transferDic.TryGetValue(playerId, out var oldTransfer))
                oldTransfer.Dispose();

            PlayerRegistryMes mes = new();
            mes.PlayerList.Add(playerId);
            mes.ClientId = playerId;

            List<int> players = new() { playerId };
            var transfer = new PlayerStateTransfer(players);
            _transferDic[playerId] = transfer;

            EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(playerId, mes));
            Console.WriteLine($"【远程同步测试】玩家注册{playerId}");
        }
        private void OnPlayerDisconnect(int playerId)
        {
            if (_transferDic.Remove(playerId, out var transfer))
            {
                Console.WriteLine($"【远程测试】玩家{playerId}断连，释放中转器");
                transfer.Dispose();
            }
        }
        public void Dispose()
        {
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
            foreach (var transfer in _transferDic.Values)
                transfer.Dispose();
            _transferDic.Clear();
        }
    }
}
