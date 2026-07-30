using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Space;

namespace Server.Scene
{
    [Serializable]
    public class StaticSceneInfo
    {
        public UnityAABB bound { get; set; }
        public UnityVector3 position { get; set; }
    }
}
