using System.Collections.Generic;
using UnityEngine;

public class EventInfo
{
    public string name;
    public List<string> addCodeList { get; private set; } = new();
    public List<string> removeCodeList { get; private set; } = new();
    public bool IsBalance => Count == 0;
    public int Count => addCodeList.Count - removeCodeList.Count;

    public EventInfo(string name) => this.name = name;

    public void AddListener(string filePath, int lineNumber)
        => addCodeList.Add($"{filePath}/{lineNumber}");

    public void RemoveListener(string filePath, int lineNumber)
    {
        if (Count <= 0)
        {
            Debug.LogWarning($"【移除监听异常】{name} 的监听计数已为 0，" +
                             $"多余的 RemoveListener 来自 {filePath}/{lineNumber}");
            return;
        }
        removeCodeList.Add($"{filePath}/{lineNumber}");
    }

    public void Clear()
    {
        addCodeList.Clear();
        removeCodeList.Clear();
    }
}
