using System;
using System.Collections.Generic;
using System.Text;
using Server.Event;
using Server.GamePlay;
using Server.GamePlay.Online;

namespace Server.Test
{
    public class LocalOnlineTest : IDisposable
    {
        private readonly Dictionary<int, Room> _roomDic = new();
        public LocalOnlineTest()
        {
            EventBus.Instance.AddListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.AddListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
        }

        public void Dispose()
        {
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerConnect, OnPlayerConnect);
            EventBus.Instance.RemoveListener<int>(EventType.OnPlayerDisconnect, OnPlayerDisconnect);
            foreach (var room in _roomDic.Values)
                room.Dispose();
            _roomDic.Clear();
        }

        private void OnPlayerConnect(int playerId)
        {
            if (_roomDic.ContainsKey(playerId))
                return;
            Room room = new();
            if (!room.Init(playerId))
            {
                Debug.LogWarning("【本地联机测试】联机房间启动失败");
                return;
            }
            _roomDic.Add(playerId, room);
            Debug.Log($"【本地联机测试】玩家注册{playerId}");
        }
        private void OnPlayerDisconnect(int playerId)
        {
            if(_roomDic.Remove(playerId,out var room))
            {
                Debug.Log($"【本地联机测试】玩家{playerId}退出");
                room.Dispose();
            }
        }
    }
}
