using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Message;
using Server.Event;

namespace Server.GamePlay.Online
{
    public class Lobby : IDisposable
    {
        public const int ROOM_PLAYER_COUNT = 1;
        public const int DELAY_UPDATE = 10;// ms
        private HashSet<int> _totalPlayerSet = new();
        private HashSet<int> _sleepPlayerSet = new();
        private HashSet<int> _waitingPlayerSet = new();

        private Dictionary<int, Room> _playingPlayersDic = new();
        private HashSet<Room> _roomSet = new();

        private ConcurrentQueue<int> _connectQueue = new();
        private ConcurrentQueue<int> _disconnectQueue = new();

        private ConcurrentQueue<int> _matchQueue = new();
        private ConcurrentQueue<int> _quitMatchQueue = new();

        private List<int> _tempPlayerList = new();
        private List<Room> _tempRoomList = new();
        private CancellationTokenSource _cancel = new();
        public Lobby()
        {
            EventBus.Instance.AddListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.AddListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
            EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnPlayerMatchMessage);
            Task.Run(UpdateLoop);
        }
        private void Update()
        {
            while (_connectQueue.TryDequeue(out int id))
                AddPlayer(id);
            while(_disconnectQueue.TryDequeue(out int id))
                RemovePlayer(id);
            while(_matchQueue.TryDequeue(out int id))
                OnPlayerMatch(id);
            while(_quitMatchQueue.TryDequeue(out int id))
                OnPlayerQuitMatch(id);

            _tempRoomList.Clear();
            foreach(var room in _roomSet)
                if(room != null && !room.IsEnable)
                    _tempRoomList.Add(room);
            foreach(var room in _tempRoomList)
                StopRoom(room);
        }
        public void Dispose()
        {
            _cancel.Cancel();
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
            EventBus.Instance.RemoveListener<ClientPackage>(EventType.OnReceive, OnPlayerMatchMessage);
        }
        private async Task UpdateLoop()
        {
            while(!_cancel.IsCancellationRequested)
            {
                try
                {
                    Update();
                }
                catch(Exception ex)
                {
                    Debug.LogError($"【游戏大厅】{DateTime.UtcNow}帧循环异常{ex.Message}");
                }
                await Task.Delay(DELAY_UPDATE);
            }
        }
        private void AddPlayer(int id)
        {
            if(_totalPlayerSet.Contains(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}重复注册");
                return;
            }
            _sleepPlayerSet.Add(id);
            _totalPlayerSet.Add(id);
        }
        private void RemovePlayer(int id)
        {
            if (!_totalPlayerSet.Contains(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}不存在");
                return;
            }
            if(!_sleepPlayerSet.Remove(id) && !_waitingPlayerSet.Remove(id))
            {
                Debug.LogWarning($"【游戏大厅】游戏中玩家{id}断联");
                if (_playingPlayersDic.TryGetValue(id, out var room))
                    StopRoom(room);
                else
                    Debug.LogWarning($"【游戏大厅】未知玩家{id}");
            }
            _totalPlayerSet.Remove(id);
        }
        private void OnPlayerMatch(int id)
        {
            if (!_totalPlayerSet.Contains(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}不存在");
                return;
            }
            if(!_sleepPlayerSet.Contains(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}已在匹配中");
                return;
            }
            _sleepPlayerSet.Remove(id);
            _waitingPlayerSet.Add(id);
            if (TryCreateRoom())
                Debug.Log("【游戏大厅】匹配成功");
        }
        private void OnPlayerQuitMatch(int id)
        {
            if (!_totalPlayerSet.Contains(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}不存在");
                return;
            }
            if(! _waitingPlayerSet.Remove(id))
            {
                Debug.LogWarning($"【游戏大厅】玩家{id}不在匹配中或已开启游戏");
                return;
            }
            _sleepPlayerSet.Add(id);
        }
        private bool TryCreateRoom()
        {
            if (_waitingPlayerSet.Count < ROOM_PLAYER_COUNT)
                return false;
            
            while (_waitingPlayerSet.Count >= ROOM_PLAYER_COUNT)
            {
                _tempPlayerList.Clear();
                foreach (var player in _waitingPlayerSet)
                {
                    _tempPlayerList.Add(player);
                    if (_tempPlayerList.Count >= ROOM_PLAYER_COUNT)
                        break;
                }

                Room room = new();
                if(!room.Init(_tempPlayerList.ToArray()))
                {
                    Debug.LogError("【游戏大厅】房间创建失败");
                    return false;
                }
                _roomSet.Add(room);
                foreach (var player in _tempPlayerList)
                {
                    _waitingPlayerSet.Remove(player);
                    _playingPlayersDic.Add(player, room);
                }
            }
            return true;
        }
        private void StopRoom(Room room)
        {
            if (room == null)
                return;
            if (_roomSet.Remove(room))
            {
                foreach (var player in room.Players)
                {
                    _sleepPlayerSet.Add(player);
                    _playingPlayersDic.Remove(player);
                    Debug.Log($"【游戏大厅】玩家{player}退出游戏状态");
                }
                room.StopRoom();
            }
            else
                Debug.LogWarning("【游戏大厅】不存在房间");
        }
        private void OnPlayerMatchMessage(ClientPackage package)
        {
            if (package.message is not PlayerMatchMessage message)
                return;
            if (message.IsMatch)
                _matchQueue.Enqueue(package.playerId);
            else
                _quitMatchQueue.Enqueue(package.playerId);
        }
        private void OnPlayerConnect(int playerId)
            => _connectQueue.Enqueue(playerId);
        private void OnPlayerDisconnect(int playerId)
            => _disconnectQueue.Enqueue(playerId);
    }
}
