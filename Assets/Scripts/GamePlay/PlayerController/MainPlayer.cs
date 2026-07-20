using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour
{
    private const string MOVE_ACTION = "Move";
    private const string ROTATION_ACTION = "Rotation";
    private const string CAMERA_ACTION = "CameraRotation";
    public PlayerController playerController;
    public float moveSpeed;
    public float rotationSpeed;
    public float cameraRotationSpeed;
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    private float _rotationXInput;
    private float _cameraRotationYInput;
    // Start is called before the first frame update
    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("【主玩家】玩家输入组件获取失败");
            return;
        }
        _playerInput.onActionTriggered += OnTrigger;

        EventBus.Instance.AddListener<float>(EventType.OnCameraPitchChange, SetPlayerPitch);
    }
    void Update()
    {
        var pos = new Vector3(_moveInput.x, 0, _moveInput.y) * moveSpeed * Time.deltaTime;
        pos = transform.rotation * pos;
        transform.position += pos;
        playerController.SetPosition(transform.position);

        transform.rotation *= Quaternion.AngleAxis(_rotationXInput * rotationSpeed * Time.deltaTime, Vector3.up);
        playerController.SetRotation(transform.rotation);

        EventBus.Instance.Trigger(EventType.CameraPitchDelta, _cameraRotationYInput * cameraRotationSpeed * Time.deltaTime);
    }
    private void OnTrigger(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case MOVE_ACTION:
                OnMove(context);
                break;
            case ROTATION_ACTION:
                OnRotation(context);
                break;
            case CAMERA_ACTION:
                OnCameraRotation(context);
                break;
        }
    }
    private void OnMove(InputAction.CallbackContext context)
    => _moveInput = context.ReadValue<Vector2>();
    private void OnRotation(InputAction.CallbackContext context)
    => _rotationXInput = context.ReadValue<float>();
    private void OnCameraRotation(InputAction.CallbackContext context)
    => _cameraRotationYInput = context.ReadValue<float>();

    private void SetPlayerPitch(float pitch)
    => playerController?.SetPitch(pitch);
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<float>(EventType.OnCameraPitchChange, SetPlayerPitch);
    }
}
