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
        IPEndPoint iPEndPoint = new(IPAddress.Parse("127.0.0.1"), 2026);
        NetServer server = new(iPEndPoint,100);
        server.StartAccept();
        while (true)
        {
            var input = Console.ReadLine();
            if (input != null && input == "0")
            {
                server.Close();
                break;
            }
        }
    }
    private static void OnTextMessage(ClientPackage package)
    {
        if (package.message is not TextMessage text)
            return;
        Console.WriteLine($"【玩家{package.id}消息】" + text.Content);
    }
}
