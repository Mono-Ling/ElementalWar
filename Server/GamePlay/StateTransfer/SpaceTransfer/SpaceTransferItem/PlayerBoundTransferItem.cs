using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Message;
using Server.Message.Tools;

namespace Server.GamePlay.StateTransfer.SpaceTransfer
{
    public class PlayerBoundTransferItem : BaseSpaceTransferItem
    {
        private const float EXPAND_TIMES = 2f;
        private ConcurrentQueue<PlayerSpaceItem> _playerBoundUpdateQueue = new();
        public override void Start(Dictionary<int, PlayerSpaceItem>? playerSpaceItemDic, SpaceTree? spaceTree)
        {
            base.Start(playerSpaceItemDic, spaceTree);
            AddListener<BoundStateMessage>(OnPlayerBoundStateSyn);
        }
        public override void Update()
        {
            while (_playerBoundUpdateQueue.TryDequeue(out var playerSpace))
                spaceTree?.UpdateItem(playerSpace);
        }
        private void OnPlayerBoundStateSyn(ClientPackage package)
        {
            if (package.message == null || package.message is not BoundStateMessage boundMes)
                return;
            if (package.header is not UdpHeader udpHeader)
                return;
            DateTime clientTime = new(udpHeader.Time);
            if (playerSpaceItemDic.TryGetValue(package.playerId, out var playerSpace))
            {
                if (playerSpace.preTime > clientTime)
                    return;
                (Vector3 center, _) = boundMes.Center;
                (Vector3 extents, _) = boundMes.Extents;

                playerSpace.bound = new(center, extents * EXPAND_TIMES);

                playerSpace.history.Add(udpHeader.Time, new(center, extents));

                _playerBoundUpdateQueue.Enqueue(playerSpace);

                playerSpace.preTime = clientTime;
            }
            else
                Console.WriteLine($"【【命中检测中转】玩家{package.playerId}不存在");
        }
        public override void Stop()
            => RemoveListener<BoundStateMessage>(OnPlayerBoundStateSyn);
    }
}
