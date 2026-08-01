using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Server.GamePlay.StateTransfer.SpaceTransfer;
using Server.Scene;

namespace Server.GamePlay.StateTransfer
{
    public class StaticSceneTransferItem : BaseSpaceTransferItem
    {
        private const string SCENE_PATH = @"D:\Unity\Project\ElementalWar\Server\Scene\Scene_1.json";
        private List<WallSpaceItem> _wallSpaceItemList = new();
        public override void Start(Dictionary<int, PlayerSpaceItem>? playerSpaceItemDic, SpaceTree? spaceTree)
        {
            base.Start(playerSpaceItemDic, spaceTree);
            LoadStaticScene();
        }
        private void LoadStaticScene()
        {
            var json = File.ReadAllText(SCENE_PATH);
            var sceneAsset = JsonSerializer.Deserialize<StaticSceneAsset>(json);
            if (sceneAsset == null)
            {
                Console.WriteLine($"【命中检测中转】场景加载失败");
                return;
            }
            for (int i = 0; i < sceneAsset.sceneInfoList.Count; i++)
            {
                var info = sceneAsset.sceneInfoList[i];
                var bound = info.bound.Switch();
                WallSpaceItem spaceItem = new(i, bound);

                _wallSpaceItemList.Add(spaceItem);
                spaceTree?.Add(spaceItem);
            }
        }
    }
}
