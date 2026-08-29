using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class DynamicTextManager : SingleMono<DynamicTextManager>
{
    private Camera _camera;
    public void StartTextManager()
    {
        _camera = Camera.main;
        var uiMgr = UIManager.Instance;
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, RemoteTextUI);
    }
    public void StopTextManager()
    => EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, RemoteTextUI);
    public void LocalShowTextUI(DynamicTextInfo info)
    {
        if (!info.IsEffective || !info.IsEffectiveWorldPosition(_camera))
            return;
        CreateDynamicText(info);
        // 网络发送

        DynamicTextMessage message = new() { Color = new(), WorldPos = new() };
        message.Color.Switch(info.color);
        message.WorldPos.Switch(info.worldPoint);
        message.Text = info.text;

        UdpHeader udpHeader = new() { IsResponse = true };
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(udpHeader, message));
    }
    public void RemoteTextUI(NetPackage package)
    {
        if (package.message is not DynamicTextMessage textMes)
            return;
        var text = textMes.Text;
        (var color, _) = textMes.Color;
        (var wPos, _) = textMes.WorldPos;
        CreateDynamicText(new(text, color, wPos));
    }
    private void CreateDynamicText(DynamicTextInfo info)
    {
        if (!info.IsEffective || !info.IsEffectiveWorldPosition(_camera))
            return;
        var text = UIManager.Instance.BufferShowUI<DynamicText>(DestroyDynamicText);
        if (text == null)
        {
            Debug.LogError("【动态文字UI管理器】文字UI创建失败");
            return;
        }
        UIManager.InitUIPosition(text);
        // 世界坐标 → 屏幕坐标 → 画布平面世界坐标
        var screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, info.worldPoint);
        if (UIManager.Instance.ScreenPointToDynamicCanvasWorld(screenPoint, out var worldPoint))
            text.transform.position = worldPoint;
        else
            Debug.LogWarning($"【动态文字UI管理器】坐标转换失败{info.worldPoint}");

        text.Init(info);
    }
    private void DestroyDynamicText(BaseUI text)
    => UIManager.Instance.BufferHideUI(text);
}
