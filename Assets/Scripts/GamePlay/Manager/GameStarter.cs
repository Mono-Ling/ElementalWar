using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public StaticSceneAsset sceneAsset;
    private MainPlayer _mainPlayer;
    void Awake()
    {
        var dynamicMgr = DynamicSceneItemMgr.Instance;
        NetManager.Instance.StartClient();
        Cursor.lockState = CursorLockMode.Confined;
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnRegistryReceive);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
        EventBus.Instance.AddListener(EventType.OnPlayerReset, OnPlayerReset);
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnRegistryReceive);
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
        EventBus.Instance.RemoveListener(EventType.OnPlayerReset, OnPlayerReset);
    }
    private void OnRegistryReceive(NetPackage package)
    {
        if (package.message is not PlayerRegistryMes message)
            return;
        StartCoroutine(StartClient(message));
    }
    public void OnGameStartReceive(NetPackage package)
    {
        if (package.message is not GameStartMessage message)
            return;
        _mainPlayer?.StartMainPlayer();
    }
    private IEnumerator StartClient(PlayerRegistryMes message)
    {
        yield return StaticSceneManager.Instance.LoadWall(sceneAsset);

        if (!CreateMainPlayer(message))
        {
            StaticSceneManager.Instance.Uninstall();
            yield break;
        }

        ManagedPlayerMgr.Instance.StartManagedPlayer();
        yield return ManagedPlayerMgr.Instance.CreateManagedPlayer(message);

        OnClientStart();

        Debug.Log("【游戏启动器】Client Start");
    }
    private void OnPlayerReset()
    {
        _mainPlayer?.EndMainPlayer();
        _mainPlayer?.InjectBlackboard();
        _mainPlayer?.StartMainPlayer();
    }
    private bool CreateMainPlayer(PlayerRegistryMes message)
    {
        if (message == null)
            return false;
        var playerViewObj = MonoObjectPool.Instance.GetObject("Player");
        if (playerViewObj == null)
        {
            Debug.LogError("【游戏启动器】主玩家显示创建失败");
            return false;
        }
        var controller = playerViewObj.GetComponent<PlayerController>();
        if (controller == null)
        {
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏启动器】主玩家显示控制器获取失败");
            return false;
        }

        var mainPlayerObj = MonoObjectPool.Instance.GetObject("MainPlayer");
        if (mainPlayerObj == null)
        {
            Debug.LogError("【游戏启动器】主玩家创建失败");
            return false;
        }
        _mainPlayer = mainPlayerObj.GetComponent<MainPlayer>();
        if (_mainPlayer == null)
        {
            MonoObjectPool.Instance.PutObject(mainPlayerObj);
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏启动器】主玩家显示控制器获取失败");
            return false;
        }
        _mainPlayer.SetMainPlayer(controller, message.ClientId);
        return true;
    }
    private void OnClientStart()
    {
        ClientStartMessage mes = new();
        EventBus.Instance.Trigger<NetPackage>(EventType.SendTo, new(mes));
    }
}
