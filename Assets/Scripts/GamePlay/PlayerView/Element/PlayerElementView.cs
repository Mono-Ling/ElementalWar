using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerElementView : MonoBehaviour
{
    private const float THRESHOLD = 0.0001f;
    public Canvas elementViewCanvas;
    private Camera _camera;
    private Blackboard _blackboard;
    private BlackboardArg<ElementAttachmentInfo> _attachmentInfoArg;

    private Dictionary<ElementType, ElementAttachmentText> _elementTextDic = new();
    private List<ElementType> _tempList = new();
    void Start()
    {
        if (elementViewCanvas == null)
        {
            Debug.LogError("【元素附着显示】元素视图Canva为空");
            return;
        }

        _camera = Camera.main;
        if (_camera == null)
            Debug.LogError("【元素附着显示】相机获取失败");

        elementViewCanvas.worldCamera = _camera;

        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【元素附着显示】黑板获取失败");
            return;
        }
        if (!_blackboard.GetBlackboardArg("ElementAttachmentInfo", out _attachmentInfoArg))
        {
            Debug.LogError("【元素附着显示】元素附着信息黑板参数获取失败");
            return;
        }
        _attachmentInfoArg.OnValueChange += OnElementAttachmentInfoChange;
    }
    private void LateUpdate()
    {
        if (_camera == null || elementViewCanvas == null) return;

        Vector3 dir = _camera.transform.position - elementViewCanvas.transform.position;
        // 消除高度差，只保留水平方向
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            elementViewCanvas.transform.rotation = Quaternion.LookRotation(-dir);
        }
    }
    private void OnElementAttachmentInfoChange(ElementAttachmentInfo info)
    {
        if (info == null)
            return;
        _tempList.Clear();
        foreach (var element in _elementTextDic.Keys)
        {
            if (!info.AttachmentInfo.TryGetValue(element, out var content)
                || content < THRESHOLD)
                _tempList.Add(element);
        }
        foreach (var element in _tempList)
        {
            if (_elementTextDic.TryGetValue(element, out var text))
            {
                UIManager.Instance.BufferHideUI(text, isAnimation: false);
                _elementTextDic.Remove(element);
            }
        }

        _tempList.Clear();
        foreach (var item in info.AttachmentInfo)
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
            text.transform.SetParent(elementViewCanvas.transform, false);
            text.SetElementAttachment(element, info.AttachmentInfo[element]);
            _elementTextDic.Add(element, text);
        }
    }
    void OnDestroy()
    {
        if (_attachmentInfoArg != null)
            _attachmentInfoArg.OnValueChange -= OnElementAttachmentInfoChange;
    }
}
