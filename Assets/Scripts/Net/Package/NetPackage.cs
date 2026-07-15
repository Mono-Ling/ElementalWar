using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;

public struct NetPackage
{
    public SendType sendType { get; private set; }
    public IMessage header { get; private set; }
    public IMessage message { get; private set; }
    public NetPackage(IMessage message, SendType sendType = SendType.Tcp)
    {
        this.message = message;
        this.sendType = sendType;
        this.header = null;
    }
    public NetPackage(IMessage header, IMessage message, SendType sendType = SendType.Udp)
    {
        this.header = header;
        this.message = message;
        this.sendType = sendType;
    }
}
