using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public StaticSceneAsset sceneAsset;
    void Awake()
    {
        var dynamicMgr = DynamicSceneItemMgr.Instance;
        NetManager.Instance.StartClient();
        Cursor.lockState = CursorLockMode.Confined;
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
    }
    private void OnGameStartReceive(NetPackage package)
    {
        if (package.message is not PlayerRegistryMes message)
            return;
        StartCoroutine(StartGame(message));
    }
    private IEnumerator StartGame(PlayerRegistryMes message)
    {
        yield return StaticSceneManager.Instance.LoadWall(sceneAsset);

        if (!CreateMainPlayer(message))
        {
            StaticSceneManager.Instance.Uninstall();
            yield break;
        }

        ManagedPlayerMgr.Instance.StartManagedPlayer();
        yield return ManagedPlayerMgr.Instance.CreateManagedPlayer(message);
        Debug.Log("【游戏启动器】Game Start");
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
        var mainPlayer = mainPlayerObj.GetComponent<MainPlayer>();
        if (mainPlayer == null)
        {
            MonoObjectPool.Instance.PutObject(mainPlayerObj);
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏启动器】主玩家显示控制器获取失败");
            return false;
        }
        mainPlayer.SetMainPlayer(controller, message.ClientId);
        return true;
    }
}
