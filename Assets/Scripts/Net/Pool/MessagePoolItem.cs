using System;
using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf;
using UnityEngine;

public class MessagePoolItem
{
    public bool Enable { get; private set; }

    // ConcurrentStack 的出栈与入栈自带内存屏障，Put 中的清理对 Pop 方可见；
    // 反序列化在出栈之后进行，不占用任何锁。同一个实例只允许归还一次。
    private readonly ConcurrentStack<IMessage> _stack = new();
    private readonly MessageParser _parser;
    public MessagePoolItem(Type type)
    {
        if (type == null)
            return;
        var parserPro = type.GetProperty("Parser", BindingFlags.Public | BindingFlags.Static);
        if (parserPro == null)
        {
            Debug.LogError($"【消息池】{type}的Protobuf解析器解析失败");
            return;
        }
        if (parserPro.GetValue(null) is not MessageParser parse)
        {
            Debug.LogError($"【消息池】{type}的目标类型Parser转换失败");
            return;
        }
        _parser = parse;
        Enable = true;
    }
    public IMessage Get(byte[] bytes, int offset, int length)
    {
        if (!Enable)
            return null;
        if (_stack.TryPop(out var message))
        {
            message.MergeFrom(bytes, offset, length);
            return message;
        }
        return _parser.ParseFrom(bytes, offset, length);
    }
    public void Put(IMessage message)
    {
        if (message == null || !Enable)
            return;
        message.Clear();
        _stack.Push(message);
    }
}
