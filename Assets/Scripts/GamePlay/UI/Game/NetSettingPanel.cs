using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetSettingPanel : BaseUI
{
    public TMP_InputField ipInputField;
    public TMP_InputField tcpPointInputField;
    public TMP_InputField udpPointInputField;
    public Button butSave;
    public Button butQuit;

    private NetSettingDTO _netDTO;
    protected override void Awake()
    {
        base.Awake();
        if (ipInputField == null)
            Debug.LogError("【网络设置面板】IP输入框为空");
        if (tcpPointInputField == null)
            Debug.LogError("【网络设置面板】TCP端口输入框为空");
        if (udpPointInputField == null)
            Debug.LogError("【网络设置面板】UDP端口输入框为空");
        if (butSave == null)
            Debug.LogError("【网络设置面板】保存按钮为空");
        if (butQuit == null)
            Debug.LogError("【网络设置面板】退出按钮为空");

        _netDTO = new(NetSettingData.Instance.DTO);
        UpdateAllInputField();

        ipInputField?.onEndEdit.AddListener(OnIPEndInput);
        tcpPointInputField?.onEndEdit.AddListener(OnTcpPointEndInput);
        udpPointInputField?.onEndEdit.AddListener(OnUdpPointEndInput);

        butSave?.onClick.AddListener(OnSave);
        butQuit?.onClick.AddListener(OnQuit);
    }
    void OnDestroy()
    {
        ipInputField?.onEndEdit.RemoveListener(OnIPEndInput);
        tcpPointInputField?.onEndEdit.RemoveListener(OnTcpPointEndInput);
        udpPointInputField?.onEndEdit.RemoveListener(OnUdpPointEndInput);

        butSave?.onClick.RemoveListener(OnSave);
        butQuit?.onClick.RemoveListener(OnQuit);
    }
    private void OnIPEndInput(string ip)
    {
        if (IPAddress.TryParse(ip, out _))
            _netDTO.ServerIp = ip;
    }
    private void OnTcpPointEndInput(string point)
    {
        if (int.TryParse(point, out int tcpPoint))
            _netDTO.TcpPort = tcpPoint;
    }
    private void OnUdpPointEndInput(string point)
    {
        if (int.TryParse(point, out int udpPoint))
            _netDTO.UdpPort = udpPoint;
    }
    private void OnSave()
    {
        NetSettingData.Instance.Apply(_netDTO);
        NetSettingData.Instance.Save();

        UpdateAllInputField();
    }
    private void OnQuit()
    => UIManager.Instance.HidePanel();
    private void UpdateAllInputField()
    {
        UpdateInputField(ipInputField, _netDTO.ServerIp);
        UpdateInputField(tcpPointInputField, _netDTO.TcpPort.ToString());
        UpdateInputField(udpPointInputField, _netDTO.UdpPort.ToString());
    }
    private void UpdateInputField(TMP_InputField inputField, string text)
    {
        if (inputField == null)
            return;
        var tmp = inputField.placeholder.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        inputField.text = string.Empty;
    }
}
