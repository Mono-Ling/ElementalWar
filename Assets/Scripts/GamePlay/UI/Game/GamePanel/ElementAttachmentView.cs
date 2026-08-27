using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementAttachmentView : MonoBehaviour, IAutoInject<Blackboard>
{
    private const float THRESHOLD = 0.0001f;
    public Vector3 scale = new(200, 200, 200);
    public Transform elementView;
    private Blackboard _blackboard;
    private BlackboardArg<ElementAttachment> _elementAttachmentArg;
    private Dictionary<ElementType, ElementAttachmentText> _elementTextDic = new();
    private List<ElementType> _tempList = new();
    void Awake()
    {
        if (elementView == null)
            Debug.LogError("【元素附着UI】元素附着视图为空");
    }
    public void AutoInject(Blackboard inject)
    {
        if (inject == null)
        {
            Debug.LogError("【元素附着UI】黑板注入为空");
            return;
        }
        _blackboard = inject;
        if (!_blackboard.GetBlackboardArg("ElementAttachment", out _elementAttachmentArg))
        {
            Debug.LogError("【元素附着UI】元素附着器组件黑板参数获取失败");
            return;
        }
        _elementAttachmentArg.OnValueChange += OnElementAttachmentChange;
        OnElementAttachmentChange(_elementAttachmentArg.value);
    }
    void OnDestroy()
    {
        if (_elementAttachmentArg != null)
            _elementAttachmentArg.OnValueChange -= OnElementAttachmentChange;
        Clear();
    }
    private void OnElementAttachmentChange(ElementAttachment attachment)
    {
        if (attachment == null)
        {
            Clear();
            return;
        }
        _tempList.Clear();
        foreach (var element in _elementTextDic.Keys)
        {
            if (!attachment.ElementContentDic.TryGetValue(element, out var content)
                || content < THRESHOLD)
                _tempList.Add(element);
        }
        foreach (var element in _tempList)
        {
            if (_elementTextDic.TryGetValue(element, out var text))
            {
                UIManager.InitUIPosition(text);
                UIManager.Instance.BufferHideUI(text, isAnimation: false);
                _elementTextDic.Remove(element);
            }
        }

        _tempList.Clear();
        foreach (var item in attachment.ElementContentDic)
        {
            if (item.Value < THRESHOLD)
                continue;

            if (_elementTextDic.TryGetValue(item.Key, out var text))
                text?.SetElementAttachment(item.Key, item.Value);
            else
                _tempList.Add(item.Key);
        }

        foreach (var element in _tempList)
        {
            var text = UIManager.Instance.BufferShowUI<ElementAttachmentText>(isAnimation: false);
            if (text == null)
            {
                Debug.LogError("【元素附着显示】元素附着文字创建失败");
                continue;
            }
            text.transform.SetParent(elementView, false);
            text.transform.localScale = scale;
            text.SetElementAttachment(element, attachment.ElementContentDic[element]);
            _elementTextDic.Add(element, text);
        }
    }
    private void Clear()
    {
        _tempList.Clear();
        foreach (var element in _elementTextDic.Keys)
            _tempList.Add(element);
        foreach (var element in _tempList)
            if (_elementTextDic.TryGetValue(element, out var text) && text != null)
                UIManager.Instance.BufferHideUI(text, isAnimation: false);
        _elementTextDic.Clear();
    }
}
