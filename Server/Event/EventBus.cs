using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Server.Event
{
    internal class EventBus
    {
        #region Singleton (double-checked locking)
        private static readonly object _instanceLock = new();
        private static EventBus? _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new EventBus();
                    }
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 直接存储 Delegate（Action 或 Action&lt;T&gt;），委托本身是不可变的，读取快照后安全调用。
        /// </summary>
        private readonly ConcurrentDictionary<EventType, Delegate?> _eventDic = new();

        #region AddListener
        public void AddListener(EventType eventType, Action action)
        {
            _eventDic.AddOrUpdate(
                eventType,
                _ => action,
                (_, existing) =>
                {
                    if (existing is Action a)
                        return a + action;
                    Console.WriteLine($"【事件总线】【监听事件】事件{eventType}类型不匹配");
                    return existing;
                });
        }

        public void AddListener<T>(EventType eventType, Action<T> action)
        {
            _eventDic.AddOrUpdate(
                eventType,
                _ => action,
                (_, existing) =>
                {
                    if (existing is Action<T> a)
                        return a + action;
                    Console.WriteLine($"【事件总线】【监听事件】事件{eventType}类型不匹配");
                    return existing;
                });
        }
        #endregion

        #region Trigger
        public void Trigger(EventType eventType)
        {
            if (_eventDic.TryGetValue(eventType, out var del) && del is Action action)
            {
                action.Invoke();
            }
            else if (del != null)
            {
                Console.WriteLine($"【事件总线】【触发事件】事件{eventType}类型不匹配");
            }
        }

        public void Trigger<T>(EventType eventType, T? arg)
        {
            if (_eventDic.TryGetValue(eventType, out var del) && del is Action<T> action)
            {
                action.Invoke(arg);
            }
            else if (del != null)
            {
                Console.WriteLine($"【事件总线】【触发事件】事件{eventType}类型不匹配");
            }
        }
        #endregion

        #region RemoveListener
        public void RemoveListener(EventType eventType, Action action)
        {
            if (!_eventDic.TryGetValue(eventType, out var existing)) return;
            if (existing is not Action)
            {
                Console.WriteLine($"【事件总线】【注销事件】事件{eventType}类型不匹配");
                return;
            }

            // CAS 循环：确保并发添加/移除场景下原子操作
            while (true)
            {
                var newValue = Delegate.Remove(existing, action);
                if (newValue == null)
                {
                    if (_eventDic.TryRemove(new KeyValuePair<EventType, Delegate?>(eventType, existing)))
                        return;
                }
                else
                {
                    if (_eventDic.TryUpdate(eventType, newValue, existing))
                        return;
                }
                // CAS 失败 → 重新读取后重试；中途若类型变化或 key 被删除则退出
                if (!_eventDic.TryGetValue(eventType, out existing) || existing is not Action)
                    return;
            }
        }

        public void RemoveListener<T>(EventType eventType, Action<T> action)
        {
            if (!_eventDic.TryGetValue(eventType, out var existing)) return;
            if (existing is not Action<T>)
            {
                Console.WriteLine($"【事件总线】【注销事件】事件{eventType}类型不匹配");
                return;
            }

            while (true)
            {
                var newValue = Delegate.Remove(existing, action);
                if (newValue == null)
                {
                    if (_eventDic.TryRemove(new KeyValuePair<EventType, Delegate?>(eventType, existing)))
                        return;
                }
                else
                {
                    if (_eventDic.TryUpdate(eventType, newValue, existing))
                        return;
                }
                if (!_eventDic.TryGetValue(eventType, out existing) || existing is not Action<T>)
                    return;
            }
        }
        #endregion
    }
}
