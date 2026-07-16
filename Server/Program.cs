using System.Net;
using Google.Protobuf;
using Message;
using Server;
using Server.Event;

class Program
{
    static void Main(string[] args)
    {
        EventBus.Instance.AddListener<ClientPackage>(EventType.OnReceive, OnTextMessage);
        IPEndPoint tcpIpEndPoint = new(IPAddress.Parse("127.0.0.1"), 2026);
        IPEndPoint udpIpEndPoint = new(IPAddress.Parse("127.0.0.1"), 2027);
        NetServer server = new();
        server.Start(tcpIpEndPoint,udpIpEndPoint);
        while (true)
        {
            var input = Console.ReadLine();
            if (input != null && input == "0")
            {
                server.Close();
                break;
            }
            UdpHeader udpHeader = new();
            udpHeader.IsResponse = true;
            TextMessage message = new();
            message.Content = input;
            ClientPackage clientPackage = new(0, udpHeader, message);
            EventBus.Instance.Trigger(EventType.SendTo, clientPackage);
        }
    }
    private static void OnTextMessage(ClientPackage package)
    {
        if (package.message is not TextMessage text)
            return;
        Console.WriteLine($"【玩家{package.playerId}消息】Content:{text.Content}|SendType:{package.sendType}");
    }
}
