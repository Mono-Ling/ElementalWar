using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using System;
using System.IO;

public class TcpPackage
{
    public bool IsCompleted => totalLength != 0 && totalLength == _currTotalLength;
    public int totalLength { get; private set; }
    public int length { get; private set; }
    public IMessage message { get; private set; }
    public byte[] data { get; private set; }
    private int _currTotalLength = 0;
    public TcpPackage(byte[] bytes, ref int offset)
    {
        if (bytes == null)
            return;
        if (bytes.Length - offset < 4)
        {
            _currTotalLength = bytes.Length - offset;
            data = new byte[_currTotalLength];
            Array.Copy(bytes, offset, data, 0, _currTotalLength);
            offset += _currTotalLength;
            return;
        }
        length = BitConverter.ToInt32(bytes, offset);
        totalLength = length + 4;
        _currTotalLength = 4;
        offset += 4;
        data = new byte[totalLength];
        if (bytes.Length - offset < length)
        {
            int curr = bytes.Length - offset;
            Array.Copy(bytes, offset, data, 4, curr);
            _currTotalLength += curr;
            offset += curr;
        }
        else
        {
            Array.Copy(bytes, offset, data, 4, length);
            offset += length;
            _currTotalLength = totalLength;

            OnCompleted();
        }
        BitConverter.GetBytes(length).CopyTo(data, 0);
    }
    public TcpPackage(IMessage message)
    {
        if (message == null)
            return;
        byte[] content = null;
        using (MemoryStream ms = new MemoryStream())
        {
            message.WriteTo(ms);
            content = ms.ToArray();
        }
        string typeStr = message.GetType().ToString();
        byte[] typeBytes = Encoding.UTF8.GetBytes(typeStr);

        length = 4 + typeBytes.Length + content.Length;
        totalLength = 4 + length;
        data = new byte[totalLength];

        BitConverter.GetBytes(length).CopyTo(data, 0);
        BitConverter.GetBytes(typeBytes.Length).CopyTo(data, 4);
        typeBytes.CopyTo(data, 8);
        content.CopyTo(data, 8 + typeBytes.Length);
    }
    public void Append(byte[] bytes, ref int offset)
    {
        if (IsCompleted)
            return;

        int curr = bytes.Length - offset;
        if (_currTotalLength < 4)
        {
            int diff = 4 - _currTotalLength;
            if (curr >= diff)
            {
                byte[] header = new byte[4];
                data.CopyTo(header, 0);
                Array.Copy(bytes, offset, header, _currTotalLength, diff);
                _currTotalLength += diff;
                offset += diff;

                length = BitConverter.ToInt32(header, 0);
                totalLength = length + 4;

                data = new byte[totalLength];
                header.CopyTo(data, 0);
            }
            else
            {
                byte[] newArr = new byte[curr + _currTotalLength];
                data.CopyTo(newArr, 0);
                Array.Copy(bytes, offset, newArr, _currTotalLength, curr);
                _currTotalLength += curr;
                offset += curr;
                return;
            }
        }

        curr = bytes.Length - offset;
        if (curr < totalLength - _currTotalLength)
        {
            Array.Copy(bytes, offset, data, _currTotalLength, curr);
            offset += curr;
            _currTotalLength += curr;
        }
        else
        {
            curr = totalLength - _currTotalLength;
            Array.Copy(bytes, offset, data, _currTotalLength, curr);
            offset += curr;
            _currTotalLength = totalLength;

            OnCompleted();
        }
    }
    private void OnCompleted()
    {
        int offseet = 4;
        int strLength = BitConverter.ToInt32(data, offseet);
        offseet += 4;
        string typeStr = Encoding.UTF8.GetString(data, offseet, strLength);
        offseet += strLength;
        Type type = Type.GetType(typeStr);
        if (type == null)
        {
            Debug.LogError($"【数据包解析失败】Type解析失败，解析字段：{typeStr}");
            return;
        }
        PropertyInfo parsePro = type.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
        if (parsePro == null)
        {
            Debug.LogError("【数据包解析失败】目标类型不含Parser属性");
            return;
        }

        if (parsePro.GetValue(null) is not MessageParser parse)
        {
            Debug.LogError("【数据包解析失败】目标类型Parser转换失败");
            return;
        }
        message = parse.ParseFrom(data, offseet, data.Length - offseet);
    }
}
