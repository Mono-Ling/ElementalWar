using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf;
using Message;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnTextMessage);
        NetManager.Instance.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TextMessage textMessage = new();
            textMessage.Content = "我他妈来了嗷";

            UdpHeader header = new();
            header.IsResponse = true;
            EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(header, textMessage));
        }
    }
    private void OnTextMessage(NetPackage package)
    {
        if (package.message is TextMessage text)
            Debug.Log($"【服务器消息】Content:{text.Content}|SendType:{package.sendType}");
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnTextMessage);
    }
}
