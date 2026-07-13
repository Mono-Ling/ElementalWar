using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;

namespace Server
{
    internal struct ClientPackage
    {
        public int id {  get; private set; }
        public IMessage? message {  get; private set; }
        public ClientPackage(int id, IMessage? message)
        {
            this.id = id;
            this.message = message;
        }
    }
}
