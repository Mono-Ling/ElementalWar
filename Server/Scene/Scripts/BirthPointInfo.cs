using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Scene.Scripts
{
    [Serializable]
    public class BirthPointInfo
    {
        public List<UnityVector3>? positions { get; set; }
    }
}
