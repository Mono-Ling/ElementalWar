using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainPlayer : MonoBehaviour, ISerializationCallbackReceiver
{
    public PlayerController playerController;

    [SerializeField]
    [SerializeReference]
    private List<BaseAbility> _abilitiesSerialized = new();

    public HashSet<BaseAbility> abilities = new();

    private PlayerInput _playerInput;
    private Blackboard _blackboard;

    public void OnBeforeSerialize()
    {
        if (abilities.Count > 0)
        {
            _abilitiesSerialized.Clear();
            _abilitiesSerialized.AddRange(abilities);
        }
    }

    public void OnAfterDeserialize()
    {
        abilities = new HashSet<BaseAbility>(_abilitiesSerialized.Where(a => a != null));
    }

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
