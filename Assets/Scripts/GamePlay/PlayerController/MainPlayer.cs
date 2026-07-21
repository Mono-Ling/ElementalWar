using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour
{
    public PlayerController playerController;
    [SerializeReference]
    public List<BaseAbility> abilities = new();
    private PlayerInput _playerInput;
    private Blackboard _blackboard;
    // Start is called before the first frame update
    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("【主玩家】玩家控制器为空");
            return;
        }
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("【主玩家】玩家输入组件获取失败");
            return;
        }
        _blackboard = playerController.blackboard;
        if (_blackboard == null)
        {
            Debug.LogError("【主玩家】主玩家黑板为空");
            return;
        }

        foreach (var ability in abilities)
            ability.InitAbility(this, _playerInput, _blackboard);
    }
    void Update()
    {
        foreach (var ability in abilities)
            ability.OnUpdate();
    }
    void LateUpdate()
    {
        foreach (var ability in abilities)
            ability.OnLateUpdate();
    }
    void FixedUpdate()
    {
        foreach (var ability in abilities)
            ability.OnFixedUpdate();
    }
    void OnDestroy()
    {
        foreach (var ability in abilities)
            ability.OnRemove();
    }
}
