using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Google.Protobuf;
using Message;
using UnityEngine;

public class UdpPackage
{
    public UdpHeader header { get; private set; }
    public IMessage message { get; private set; }
    public byte[] data { get; private set; }
    public UdpPackage(byte[] data)
    {
        if (data == null || data.Length < 4)
            return;
        int offset = 0;
        int headerLength = BitConverter.ToInt32(data, offset);
        offset += 4;
        // header = UdpHeader.Parser.ParseFrom(data, offset, headerLength);
        var headerMes = MessagePool.Instance.Get(typeof(UdpHeader), data, offset, headerLength);
        if (headerMes is not UdpHeader udpHeader)
        {
            Debug.LogError("【UDP消息包】UDP消息头创建失败");
            return;
        }
        header = udpHeader;
        offset += headerLength;
        if (header == null)
        {
            Debug.LogError("【UDP消息解析失败】消息头解析失败");
            return;
        }
        Type type = Type.GetType(header.Type);
        if (type == null)
        {
            Debug.LogError("【UDP消息解析失败】Type解析失败");
            return;
        }

        message = MessagePool.Instance.Get(type, data, offset, data.Length - offset);
    }
    public UdpPackage(UdpHeader header, IMessage message)
    {
        if (header == null || message == null)
            return;
        this.message = message;
        this.header = header;
        byte[] headerBytes = null;
        using (MemoryStream ms = new MemoryStream())
        {
            header.WriteTo(ms);
            headerBytes = ms.ToArray();
        }

        byte[] dataBytes = null;
        using (MemoryStream ms = new MemoryStream())
        {
            message.WriteTo(ms);
            dataBytes = ms.ToArray();
        }
        int totalLength = 4 + headerBytes.Length + dataBytes.Length;

        data = new byte[totalLength];
        int offset = 0;
        BitConverter.GetBytes(headerBytes.Length).CopyTo(data, offset);
        offset += 4;
        headerBytes.CopyTo(data, offset);
        offset += headerBytes.Length;
        dataBytes.CopyTo(data, offset);
    }
}
