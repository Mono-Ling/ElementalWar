using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.Scene
{
    public struct UnityVector3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Vector3 Switch()
        => new Vector3(x, y, z);
        public override string ToString() => $"({x},{y},{z})";
    }
}
