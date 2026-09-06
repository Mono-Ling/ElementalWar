using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

/// <summary>
/// 网络设置的 JSON 数据载体，与 NetSettingData 分离
/// </summary>
[Serializable]
public class NetSettingDTO
{
    public string ServerIp = NetSettingData.DEFAULT_IP;
    public int TcpPort = NetSettingData.DEFAULT_TCP_POINT;
    public int UdpPort = NetSettingData.DEFAULT_UDP_POINT;
}

public class NetSettingData : SettingData<NetSettingData>
{
    public const string DEFAULT_IP = "127.0.0.1";
    public const int DEFAULT_TCP_POINT = 2026;
    public const int DEFAULT_UDP_POINT = 2027;
    public IPEndPoint ServerTCP => _serverTcp;
    public IPEndPoint ServerUDP => _serverUdp;
    private IPEndPoint _serverTcp;
    private IPEndPoint _serverUdp;
    private NetSettingDTO _data = new();
    private NetSettingData()
    => Load();
    public override void Load()
    {
        if (!ExistFile(Application.persistentDataPath,
                    "SettingData",
                    $"{typeof(NetSettingData).Name}.json"))
        {
            Reset();
            return;
        }
        var fullPath = Path.Combine(Application.persistentDataPath,
                                    "SettingData",
                                    $"{typeof(NetSettingData).Name}.json");
        try
        {
            var json = File.ReadAllText(fullPath);
            var data = JsonUtility.FromJson<NetSettingDTO>(json);
            if (data == null || string.IsNullOrEmpty(data.ServerIp))
            {
                Debug.LogError($"【网络设置信息】加载失败{fullPath}");
                Reset();
                return;
            }
            Apply(data);
        }
        catch (Exception e)
        {
            // 配置文件损坏或 IP 非法等异常，恢复默认配置避免启动崩溃
            Debug.LogError($"【网络设置信息】配置解析异常：{e.Message}");
            Reset();
        }
    }
    public override void Save()
    {
        EnsureFolder(Application.persistentDataPath, "SettingData");
        var fullPath = Path.Combine(Application.persistentDataPath,
                                    "SettingData",
                                    $"{typeof(NetSettingData).Name}.json");
        var json = JsonUtility.ToJson(_data);
        File.WriteAllText(fullPath, json);

        Debug.Log($"【网络设置信息】保存网络设置 {fullPath}");
    }
    private void Apply(NetSettingDTO data)
    {
        _data = data;
        var ip = IPAddress.Parse(data.ServerIp);
        _serverTcp = new(ip, data.TcpPort);
        _serverUdp = new(ip, data.UdpPort);
    }
    private void Reset()
    {
        Apply(new NetSettingDTO());
        Save();
    }
}
