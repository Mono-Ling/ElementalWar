using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilitySystem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    [SerializeReference]
    private List<BaseAbility> _abilitiesSerialized = new();
    public bool IsStart { get; private set; }
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
    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
            Debug.LogError("【【AbilitySystem】玩家输入组件获取失败");
    }
    public void StartAbilitySystem(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【AbilitySystem】主玩家黑板为空，初始化失败");
            return;
        }

        this._blackboard = blackboard;
        IsStart = true;

        foreach (var ability in abilities)
            ability.InitAbility(this, _playerInput, _blackboard);
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsStart)
            return;
        foreach (var ability in abilities)
            ability.OnUpdate();
    }
    void LateUpdate()
    {
        if (!IsStart)
            return;
        foreach (var ability in abilities)
            ability.OnLateUpdate();
    }
    void FixedUpdate()
    {
        if (!IsStart)
            return;
        foreach (var ability in abilities)
            ability.OnFixedUpdate();
    }
    void OnDestroy()
    {
        if (!IsStart)
            return;
        foreach (var ability in abilities)
            ability.OnRemove();
    }
    public void SetAbilities(List<BaseAbility> abilitityList)
    {
        foreach (var ability in abilities)
            ability.OnRemove();

        abilities.Clear();
        foreach (var ability in abilitityList)
            if (ability != null)
                abilities.Add(ability);

        foreach (var ability in abilities)
            ability.InitAbility(this, _playerInput, _blackboard);
    }
}
