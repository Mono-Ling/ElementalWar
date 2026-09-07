using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDisConnectedState", menuName = "StateMachine/State/Game/DisConnectedState")]
public class DisConnectedState : State
{
    private Camera _camera;
    public override void OnEnter(Blackboard blackboard)
    {
        _camera = Camera.main;
        if (_camera != null)
        {
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.black;
        }
        UIManager.Instance.ClearPanel();
        UIManager.Instance.ShowPanel<InitPanel>();
    }
    public override void OnExit(Blackboard blackboard)
    {
        UIManager.Instance.ClearPanel();
        if (_camera != null)
            _camera.clearFlags = CameraClearFlags.Skybox;
    }
}
