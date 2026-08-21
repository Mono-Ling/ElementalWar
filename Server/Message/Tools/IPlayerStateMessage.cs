using System;
using System.Collections.Generic;
using System.Text;

namespace Message
{
    public interface IPlayerStateMessage
    {
        int PlayerId { get; set; }
    }
    public partial class JumpStateMessage : IPlayerStateMessage { }
    public partial class ShootStateMessage : IPlayerStateMessage { }
    public partial class ThrowStateMessage : IPlayerStateMessage { }
    public partial class ElementAttachmentMessage : IPlayerStateMessage { }
    public partial class ElementShieldViewStateMessage : IPlayerStateMessage { }
}
