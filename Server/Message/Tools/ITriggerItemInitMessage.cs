using System;
using System.Collections.Generic;
using System.Text;

namespace Message
{
    public interface ITriggerItemInitMessage
    {
        BoundStateMessage Bound { get; set; }
    }
    public partial class ElementCrystalInitMessage : ITriggerItemInitMessage { }
}
