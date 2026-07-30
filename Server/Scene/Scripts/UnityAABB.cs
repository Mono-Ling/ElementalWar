using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Space;

namespace Server.Scene
{
    public struct UnityAABB
    {
        public UnityVector3 center { get; set; }
        public UnityVector3 extents { get; set; }
        public AABB Switch()
        => new(center.Switch(), extents.Switch());
        public override string ToString() => $"{center}|{extents}";
    }
}
