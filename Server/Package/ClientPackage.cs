using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Server
{
    internal struct ClientPackage
    {
        public int id {  get; private set; }
        public SendType sendType { get; private set; }
        public IMessage? header {  get; private set; }
        public IMessage? message {  get; private set; }
        public ClientPackage(int id, IMessage? message,SendType sendType = SendType.Tcp)
        {
            this.id = id;
            this.message = message;
            this.sendType = sendType;
        }
        public ClientPackage(int id,IMessage? header,IMessage? message,SendType sendType = SendType.Udp)
        {
            this.id = id;
            this.header = header;
            this.message = message;
            this.sendType = sendType;
        }
        public void SetHeader(IMessage? header)
        {
            this.header = header;
        }
    }
}
