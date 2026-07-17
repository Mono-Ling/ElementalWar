using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
    public BlackboardTemplate argTemplate;
    private Dictionary<string, BaseBlackboardArg> _argDic;
    void Awake()
    {
        if (argTemplate == null)
        {
            Debug.LogError("【状态机黑板】状态机参数模板为空");
            return;
        }
        _argDic = argTemplate.GetArgDic();
    }
    public bool GetValue<T>(string name, out T value)
    {
        var temp = default(T);
        bool result = VisitArgDic<T>(name, (arg) => { temp = arg.value; });
        value = temp;
        return result;
    }
    public bool SetValue<T>(string name, T value)
    {
        return VisitArgDic<T>(name, (arg) => { arg.value = value; });
    }
    public bool GetBlackboardArg<T>(string name, out BlackboardArg<T> arg)
    {
        BlackboardArg<T> temp = null;
        bool result = VisitArgDic<T>(name, (a) => { temp = a; });
        arg = temp;
        return result;
    }
    private bool VisitArgDic<T>(string name, Action<BlackboardArg<T>> action)
    {
        if (_argDic.TryGetValue(name, out var baseArg))
        {
            if (baseArg is BlackboardArg<T> arg)
            {
                action?.Invoke(arg);
                return true;
            }
            else
            {
                Debug.LogWarning($"【状态机黑板】参数:{name}类型不匹配，实际传入：{typeof(T)}");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"【状态机黑板】不存在参数:{name}");
            return false;
        }
    }
    void OnDestroy()
    {
        if (_argDic == null) return;
        foreach (var baseArg in _argDic.Values)
            if (baseArg is IClearAllListeners arg)
                arg.ClearAllListeners();
        _argDic.Clear();
    }
}
public interface IClearAllListeners
{
    void ClearAllListeners();
}
public class BaseBlackboardArg { }
public class BlackboardArg<T> : BaseBlackboardArg, IClearAllListeners
{
    private T _value;
    public T value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChange?.Invoke(_value);
            }
        }
    }
    public event Action<T> OnValueChange;

    public void ClearAllListeners()
    {
        OnValueChange = null;
    }
}