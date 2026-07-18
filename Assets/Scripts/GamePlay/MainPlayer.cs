using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour
{
    public PlayerController playerController;
    public float speed;
    private PlayerInput _playerInput;
    private Vector2 _moveInput;
    // Start is called before the first frame update
    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("【主玩家】玩家输入组件获取失败");
            return;
        }
        _playerInput.onActionTriggered += OnMove;
    }
    void Update()
    {
        var pos = new Vector3(_moveInput.x, 0, _moveInput.y) * speed * Time.deltaTime;
        transform.position += pos;
        playerController.SetPosition(transform.position);
    }
    private void OnMove(InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
}
