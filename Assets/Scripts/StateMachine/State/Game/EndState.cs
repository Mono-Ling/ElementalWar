using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏流程-结束状态
/// 显示战绩，清理场景
/// </summary>
[CreateAssetMenu(fileName = "NewEndState", menuName = "StateMachine/State/Game/EndState")]
public class EndState : State
{
    public override void OnEnter(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【游戏流程-结束状态】OnEnter黑板为空");
            return;
        }
        UIManager.Instance.ClearPanel();
        ClearScene(blackboard);
        blackboard.SetValue("IsQuit", true);
    }
    private void ClearScene(Blackboard blackboard)
    {
        if (blackboard != null &&
            blackboard.GetValue<MainPlayer>("MainPlayer", out var mainPlayer))
        {
            mainPlayer?.EndMainPlayer();
            MonoObjectPool.Instance.PutObject(mainPlayer.gameObject, ResetPosition);
        }
        if (blackboard != null &&
            blackboard.GetValue<GameObject>("PlayerView", out var obj))
            MonoObjectPool.Instance.PutObject(obj, ResetPosition);

        DynamicSceneItemMgr.Instance.ClearLocal();
        DynamicSceneItemMgr.Instance.ClearRemote();
        StaticSceneManager.Instance.Uninstall();
        SceneBKManager.Instance.Uninstall();
        ManagedPlayerMgr.Instance.StopManagedPlayer();
    }
    private void ResetPosition(GameObject obj)
    {
        if (obj == null)
            return;
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
    }
}
