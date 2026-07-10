using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EventBus : Single<EventBus>
{
    private abstract class BaseEventItem
    {
        public Type type;
        public abstract List<EventInfo> GetEventInfos();
    }

    // ==================== 泛型版本（单参数事件） ====================
    private class EventItem<T> : BaseEventItem
    {
        public Dictionary<Action<T>, EventInfo> actionDic = new();
        private List<Action<T>> _loseActions = new();
        private Dictionary<Action<T>, EventInfo> _pendingAdds = new();
        private bool _isTrigging;

        public EventItem(Type type)
        {
            this.type = type;
        }

        public void Trigger(T arg)
        {
            _isTrigging = true;
            try
            {
                foreach (var item in actionDic)
                {
                    var info = item.Value;
                    int count = Math.Max(0, info.Count);
                    for (int i = 0; i < count; i++)
                        item.Key?.Invoke(arg);
                }
            }
            finally
            {
                ClearLoseAction();
                ProcessPendingAdds();
                _isTrigging = false;
            }
        }

        public void AddListener(Action<T> action, string filePath, int lineNumber)
        {
            // 已存在：递增引用计数
            if (actionDic.TryGetValue(action, out var info))
            {
                info.AddListener(filePath, lineNumber);
                return;
            }

            // 正在触发中：检查待添加队列，防止重复添加
            if (_isTrigging && _pendingAdds.TryGetValue(action, out var pendingInfo))
            {
                pendingInfo.AddListener(filePath, lineNumber);
                return;
            }

            var newInfo = new EventInfo(action.ToString());
            newInfo.AddListener(filePath, lineNumber);

            if (_isTrigging)
                _pendingAdds.Add(action, newInfo);
            else
                actionDic.Add(action, newInfo);
        }

        public void RemoveListener(Action<T> action, string filePath, int lineNumber)
        {
            if (actionDic.TryGetValue(action, out var info))
            {
                info.RemoveListener(filePath, lineNumber);
                if (info.IsBalance)
                {
                    if (_isTrigging)
                        _loseActions.Add(action);
                    else
                        actionDic.Remove(action);
                }
            }
            else
            {
                Debug.LogWarning($"【移除监听失败】Action 未注册，无法移除：{action}");
            }
        }

        public override List<EventInfo> GetEventInfos()
        {
            List<EventInfo> infos = new();
            foreach (var item in actionDic.Values)
                infos.Add(item);
            return infos;
        }

        private void ClearLoseAction()
        {
            foreach (var item in _loseActions)
                if (actionDic[item].Count == 0)
                    actionDic.Remove(item);
            _loseActions.Clear();
        }

        private void ProcessPendingAdds()
        {
            foreach (var kvp in _pendingAdds)
                actionDic[kvp.Key] = kvp.Value;
            _pendingAdds.Clear();
        }
    }

    // ==================== 非泛型版本（无参事件） ====================
    private class EventItem : BaseEventItem
    {
        public Dictionary<Action, EventInfo> actionDic = new();
        private List<Action> _loseActions = new();
        private Dictionary<Action, EventInfo> _pendingAdds = new();
        private bool _isTrigging;

        public void Trigger()
        {
            _isTrigging = true;
            try
            {
                foreach (var item in actionDic)
                {
                    var info = item.Value;
                    int count = Math.Max(0, info.Count);
                    for (int i = 0; i < count; i++)
                        item.Key?.Invoke();
                }
            }
            finally
            {
                ClearLoseAction();
                ProcessPendingAdds();
                _isTrigging = false;
            }
        }

        public void AddListener(Action action, string filePath, int lineNumber)
        {
            if (actionDic.TryGetValue(action, out var info))
            {
                info.AddListener(filePath, lineNumber);
                return;
            }

            if (_isTrigging && _pendingAdds.TryGetValue(action, out var pendingInfo))
            {
                pendingInfo.AddListener(filePath, lineNumber);
                return;
            }

            var newInfo = new EventInfo(action.ToString());
            newInfo.AddListener(filePath, lineNumber);

            if (_isTrigging)
                _pendingAdds.Add(action, newInfo);
            else
                actionDic.Add(action, newInfo);
        }

        public void RemoveListener(Action action, string filePath, int lineNumber)
        {
            if (actionDic.TryGetValue(action, out var info))
            {
                info.RemoveListener(filePath, lineNumber);
                if (info.IsBalance)
                {
                    if (_isTrigging)
                        _loseActions.Add(action);
                    else
                        actionDic.Remove(action);
                }
            }
            else
            {
                Debug.LogWarning($"【移除监听失败】Action 未注册，无法移除：{action}");
            }
        }

        public override List<EventInfo> GetEventInfos()
        {
            List<EventInfo> infos = new();
            foreach (var item in actionDic.Values)
                infos.Add(item);
            return infos;
        }

        private void ClearLoseAction()
        {
            foreach (var item in _loseActions)
                if (actionDic[item].Count == 0)
                    actionDic.Remove(item);
            _loseActions.Clear();
        }

        private void ProcessPendingAdds()
        {
            foreach (var kvp in _pendingAdds)
                actionDic[kvp.Key] = kvp.Value;
            _pendingAdds.Clear();
        }
    }

    // ==================== EventBus 公开 API ====================
    private Dictionary<EventType, BaseEventItem> eventDic = new();

    public void AddListener(EventType eventType, Action action,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem item)
                item.AddListener(action, filePath, lineNumber);
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：无参");
        }
        else
        {
            var eventItem = new EventItem();
            eventItem.AddListener(action, filePath, lineNumber);
            eventDic.Add(eventType, eventItem);
        }
    }

    public void AddListener<T>(EventType eventType, Action<T> action,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem<T> item)
                item.AddListener(action, filePath, lineNumber);
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：{typeof(T)}");
        }
        else
        {
            var eventItem = new EventItem<T>(typeof(T));
            eventItem.AddListener(action, filePath, lineNumber);
            eventDic.Add(eventType, eventItem);
        }
    }

    public void Trigger(EventType eventType)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem item)
                item.Trigger();
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：无参");
        }
        else
            Debug.LogWarning($"【事件暂无监听】事件：{eventType} 无监听");
    }

    public void Trigger<T>(EventType eventType, T arg)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem<T> item)
                item.Trigger(arg);
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：{typeof(T)}");
        }
        else
            Debug.LogWarning($"【事件暂无监听】事件：{eventType} 无监听");
    }

    public void RemoveListener(EventType eventType, Action action,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem item)
                item.RemoveListener(action, filePath, lineNumber);
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：无参");
        }
        else
            Debug.LogWarning($"【事件暂无监听】事件：{eventType} 无监听");
    }

    public void RemoveListener<T>(EventType eventType, Action<T> action,
        [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (eventDic.TryGetValue(eventType, out var baseItem))
        {
            if (baseItem is EventItem<T> item)
                item.RemoveListener(action, filePath, lineNumber);
            else
                Debug.LogError($"【事件参数不匹配】期待参数：{baseItem.type}，实际传入类型：{typeof(T)}");
        }
        else
            Debug.LogWarning($"【事件暂无监听】事件：{eventType} 无监听");
    }

    public void Clear() => eventDic.Clear();

    public void Clear(EventType eventType)
    {
        if (eventDic.ContainsKey(eventType))
            eventDic.Remove(eventType);
        else
            Debug.LogWarning($"【事件暂无监听】事件：{eventType} 无监听");
    }

    public List<EventInfo> GetEventInfos(EventType eventType)
    {
        if (eventDic.TryGetValue(eventType, out var value))
            return value.GetEventInfos();
        else
            return null;
    }
}
