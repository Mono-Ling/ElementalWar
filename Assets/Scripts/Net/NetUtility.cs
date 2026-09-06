using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public static class NetUtility
{
    public static IPAddress GetLocalIPv4()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                return ip;
        Debug.LogError("【网络工具】本机IP获取失败");
        return IPAddress.Parse("127.0.0.1");
    }
}
