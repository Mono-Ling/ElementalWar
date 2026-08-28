using System;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.Online
{
    public class Room : IDisposable
    {
        public const float TIME = 10f;// s
        public bool IsEnable => _isEnable;
        public IEnumerable<int> Players => _playerDic.Keys;
        private Dictionary<int,bool> _playerDic = new();
        private int _startClientCount;
        private PlayerStateTransfer? _playerStateTransfer;
        private bool _isEnable;
        private CancellationTokenSource _cancel = new();
        public bool Init(params int[] playerList)
        {
            if (playerList == null || playerList.Length == 0)
                return false;

            foreach (var player in playerList)
            {
                if (_playerDic.ContainsKey(player))
                {
                    Debug.LogWarning($"【联机房间】玩家{player}重复注册");
                    return false;
                }
                _playerDic.Add(player, false);

                PlayerRegistryMes registryMes = new() { ClientId = player };
                registryMes.PlayerList.AddRange(playerList);
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(player, registryMes));
            }

            _playerStateTransfer = new(new(playerList));
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnClientStart);
            _isEnable = true;

            Task.Run(Delay);
            Debug.Log("【联机房间】房间初始化完成");
            return true;
        }
        public void Dispose()
        {
            _cancel.Cancel();
            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnClientStart);
            _playerStateTransfer?.Dispose();
        }
        public void StopRoom()
        {
            GameStateMessage mes = new() { IsStart = false };
            foreach (int player in _playerDic.Keys)
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo, new(player, mes));
            _isEnable = false;
            Dispose();
            Debug.Log("【联机房间】房间关闭");
        }
        private async Task Delay()
        {
            try
            {
                await Task.Delay((int)(TIME * 1000), _cancel.Token);
                _isEnable = false;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogError($"Room Delay异常 {ex}");
            }
        }

        private void OnClientStart(ClientPackage package)
        {
            if (package.message is not ClientStartMessage message)
                return;
            if(_playerDic.TryGetValue(package.playerId,out var player))
            {
                if(player)
                {
                    Debug.LogWarning($"【联机房间】玩家{package.playerId}已启动");
                    return;
                }
                _playerDic[package.playerId] = true;
                if (++_startClientCount == _playerDic.Count)
                    StartRoom();
                Debug.Log($"【联机房间】玩家{package.playerId}客户端启动");
            }
        }
        private void StartRoom()
        {
            _playerStateTransfer?.Start();
            GameStateMessage startMessage = new() { IsStart = true };
            foreach(int player in _playerDic.Keys)
                EventBus.Instance.Trigger<ClientPackage>(EventType.SendTo,new(player,startMessage));

            Debug.Log("【联机房间】房间启动");
        }
    }
}
