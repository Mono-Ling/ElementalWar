using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Space;

namespace Server.GamePlay.StateTransfer
{
    public class SpaceStateTransfer : BaseTransfer
    {
        // 场景常量
        private const float SCENE_CENTER_OFFSET = 10;
        private const float SCENE_X = 100;
        private const float SCENE_Y = 20;
        private const float SCENE_Z = 100;

        private Dictionary<int, PlayerSpaceItem> _playerSpaceItemDic = new();
        public SpaceTree spaceTree { get; private set; } = new(new AABB(Vector3.UnitY * SCENE_CENTER_OFFSET, new(SCENE_X, SCENE_Y, SCENE_Z)));
        private List<BaseSpaceTransferItem> _spaceTransferItemList = new();
        public SpaceStateTransfer()
        {
            _spaceTransferItemList.Add(new StaticSceneTransferItem());
            _spaceTransferItemList.Add(new PlayerBoundTransferItem());
            _spaceTransferItemList.Add(new ShootHitTransferItemcs());
            _spaceTransferItemList.Add(new ExplosionHitTransferItem());
        }

        public override void Start(PlayerStateTransfer? playerStateTransfer, List<int> playerList)
        {
            base.Start(playerStateTransfer, playerList);
            foreach (int player in playerList)
            {
                if (_playerSpaceItemDic.ContainsKey(player))
                {
                    Console.WriteLine($"【命中检测中转】玩家{player}重复注册");
                    continue;
                }
                PlayerSpaceItem spaceItem = new(player);
                _playerSpaceItemDic.Add(player, spaceItem);
                spaceTree.Add(spaceItem);
            }

            foreach (var item in _spaceTransferItemList)
            {
                item.Start(playerStateTransfer, playerList);
                item.Start(_playerSpaceItemDic,spaceTree);
            }
        }
        public override void Update()
        {
            foreach (var item in _spaceTransferItemList)
                item.Update();
        }
        public override void Stop()
        {
            foreach(var item in _spaceTransferItemList)
                item.Stop();
        }
    }
}
