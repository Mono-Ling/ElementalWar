using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLoadingState", menuName = "StateMachine/State/Game/LoadingState")]
public class LoadingState : State
{
    public StaticSceneAsset sceneAsset;
    private Blackboard _blackboard;
    private MainPlayer _mainPlayer;
    private Coroutine _coroutine;
    protected override void OnValidate()
    {
        base.OnValidate();
        if (sceneAsset == null)
            Debug.LogError("【游戏流程-加载状态】场景资源为空");
    }
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【游戏流程-加载状态】OnEnter黑板为空");
            return;
        }
        _blackboard = blackboard;
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnRegistryReceive);
        EventBus.Instance.AddListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
    }
    public override void OnExit(Blackboard blackboard)
    {
        if (_coroutine != null)
        {
            PublicMono.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnRegistryReceive);
        EventBus.Instance.RemoveListener<NetPackage>(EventType.OnReceive, OnGameStartReceive);
    }
    private void OnRegistryReceive(NetPackage package)
    {
        if (package.message is not PlayerRegistryMes message || _coroutine != null)
            return;
        _coroutine = PublicMono.Instance.StartCoroutine(StartClient(message));
    }
    public void OnGameStartReceive(NetPackage package)
    {
        if (package.message is not GameStateMessage message || !message.IsStart)
            return;
        _blackboard?.SetValue("IsGameStart", true);
    }
    private IEnumerator StartClient(PlayerRegistryMes message)
    {
        yield return StaticSceneManager.Instance.LoadWall(sceneAsset);

        if (!CreateMainPlayer(message, out var viewObj))
        {
            StaticSceneManager.Instance.Uninstall();
            _coroutine = null;
            yield break;
        }
        _blackboard?.SetValue("MainPlayer", _mainPlayer);
        _blackboard?.SetValue("PlayerView", viewObj);

        ManagedPlayerMgr.Instance.StartManagedPlayer();
        yield return ManagedPlayerMgr.Instance.CreateManagedPlayer(message);

        OnClientStart();
        _coroutine = null;

        Debug.Log("【游戏流程-加载状态】Client Start");
    }
    private bool CreateMainPlayer(PlayerRegistryMes message, out GameObject playerViewObj)
    {
        playerViewObj = default;
        if (message == null)
            return false;
        playerViewObj = MonoObjectPool.Instance.GetObject("Player");
        if (playerViewObj == null)
        {
            Debug.LogError("【游戏流程-加载状态】主玩家显示创建失败");
            return false;
        }
        var controller = playerViewObj.GetComponent<PlayerController>();
        if (controller == null)
        {
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏流程-加载状态】主玩家显示控制器获取失败");
            return false;
        }

        var mainPlayerObj = MonoObjectPool.Instance.GetObject("MainPlayer");
        if (mainPlayerObj == null)
        {
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏流程-加载状态】主玩家创建失败");
            return false;
        }
        _mainPlayer = mainPlayerObj.GetComponent<MainPlayer>();
        if (_mainPlayer == null)
        {
            MonoObjectPool.Instance.PutObject(mainPlayerObj);
            MonoObjectPool.Instance.PutObject(playerViewObj);
            Debug.LogError("【游戏流程-加载状态】主玩家显示控制器获取失败");
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
