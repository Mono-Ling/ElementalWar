using System.Net;
using Google.Protobuf;
using Message;
using Server;
using Server.Event;

class Program
{
    static void Main(string[] args)
    {
        EventBus.Instance.AddListener<IMessage>(EventType.OnReceive, OnTextMessage);
        IPEndPoint iPEndPoint = new(IPAddress.Parse("127.0.0.1"), 2026);
        NetServer server = new(iPEndPoint,100);
        while (true) { }
    }
    private static void OnTextMessage(IMessage message)
    {
        if (message is not TextMessage text)
            return;
        Console.WriteLine(text.Content);
    }
}
