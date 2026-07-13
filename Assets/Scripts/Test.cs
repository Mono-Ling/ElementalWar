using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Message;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventBus.Instance.AddListener<IMessage>(EventType.OnReceive, OnTextMessage);
        NetManager.Instance.StartClient();
        var heart = HeartManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // TextMessage textMessage = new();
            // textMessage.Content = "我他妈来了嗷";
            // NetManager.Instance.Send(new TcpPackage(textMessage));
            NetManager.Instance.Close();
        }
    }
    private void OnTextMessage(IMessage message)
    {
        if (message is TextMessage text)
            Debug.Log("【服务器消息】" + text.Content);
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<IMessage>(EventType.OnReceive, OnTextMessage);
    }
}
