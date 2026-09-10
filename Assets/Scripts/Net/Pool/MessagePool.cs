using System;
using System.Collections.Concurrent;
using Google.Protobuf;

public class MessagePool : Single<MessagePool>
{
    private readonly ConcurrentDictionary<Type, MessagePoolItem> _poolDic = new();
    public IMessage Get(Type type, byte[] bytes, int offset, int length)
    {
        if (type == null)
            return null;
        if (!_poolDic.TryGetValue(type, out var item))
            item = _poolDic.GetOrAdd(type, static t => new MessagePoolItem(t));
        return item.Get(bytes, offset, length);
    }
    public void Put(IMessage message)
    {
        if (message == null)
            return;
        if (_poolDic.TryGetValue(message.GetType(), out var item))
            item.Put(message);
    }
}
