using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Event
{
    internal class EventBus
    {
        private static object _lockObj = new();
        private static EventBus? _instance;
        public static EventBus Instance => _instance ?? (_instance = new EventBus());
        private abstract class BaseEventItem { }
        private class EventItem : BaseEventItem
        {
            public event Action action;
            public EventItem(Action action) => this.action = action;
            public void Trigger() => action?.Invoke();
        }
        private class EventItem<T> : BaseEventItem
        {
            public event Action<T> action;
            public EventItem(Action<T> action) => this.action = action;
            public void Trigger(T arg) => action?.Invoke(arg);
        }
        private Dictionary<EventType, BaseEventItem> _eventDic = new();
        public void AddListener(EventType eventType,Action action)
        {
            lock (_lockObj)
            {
                if (_eventDic.ContainsKey(eventType))
                {
                    if (_eventDic[eventType] is EventItem item)
                    {
                        item.action += action;
                    }
                    else
                        Console.WriteLine($"【事件总线】【监听事件】事件{eventType}类型不匹配");
                }
                else
                    _eventDic.Add(eventType, new EventItem(action));
            }
        }
        public void AddListener<T>(EventType eventType, Action<T> action)
        {
            lock (_lockObj)
            {
                if (_eventDic.ContainsKey(eventType))
                {
                    if (_eventDic[eventType] is EventItem<T> item)
                    {
                        item.action += action;
                    }
                    else
                        Console.WriteLine($"【事件总线】【监听事件】事件{eventType}类型不匹配");
                }
                else
                    _eventDic.Add(eventType, new EventItem<T>(action));
            }
        }
        public void Trigger(EventType eventType)
        {
            if( _eventDic.ContainsKey(eventType))
            {
                if (_eventDic[eventType] is EventItem item)
                {
                    item.Trigger();
                }
                else
                    Console.WriteLine($"【事件总线】【触发事件】事件{eventType}类型不匹配");
            }
        }
        public void Trigger<T>(EventType eventType,T? arg)
        {
            if (_eventDic.ContainsKey(eventType))
            {
                if (_eventDic[eventType] is EventItem<T> item)
                {
                    item.Trigger(arg);
                }
                else
                    Console.WriteLine($"【事件总线】【触发事件】事件{eventType}类型不匹配");
            }
        }
        public void RemoveListener(EventType eventType, Action action)
        {
            lock (_lockObj)
            {
                if (_eventDic.ContainsKey(eventType))
                {
                    if (_eventDic[eventType] is EventItem item)
                        item.action -= action;
                    else
                        Console.WriteLine($"【事件总线】【注销事件】事件{eventType}类型不匹配");
                }
            }
        }
        public void RemoveListener<T>(EventType eventType, Action<T> action)
        {
            lock (_lockObj)
            {
                if (_eventDic.ContainsKey(eventType))
                {
                    if (_eventDic[eventType] is EventItem<T> item)
                        item.action -= action;
                    else
                        Console.WriteLine($"【事件总线】【注销事件】事件{eventType}类型不匹配");
                }
            }
        }
    }
}
