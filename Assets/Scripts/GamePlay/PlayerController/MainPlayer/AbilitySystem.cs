using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(ManualStateMachine))]
public class AbilitySystem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    [SerializeReference]
    private List<BaseAbility> _abilitiesSerialized = new();
    public bool IsStart { get; private set; }
    public HashSet<BaseAbility> abilities = new();
    private PlayerInput _playerInput;
    private Blackboard _blackboard;
    private ManualStateMachine _manualStateMachine;

    private List<BaseAbility> _abilitiesToRemove = new();
    private List<BaseAbility> _abilitiesToAdd = new();
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
        _manualStateMachine = GetComponent<ManualStateMachine>();
        if (_manualStateMachine == null)
            Debug.LogError("【AbilitySystem】手动状态机组件获取失败");
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

        blackboard.SetValue<AbilitySystem>("AbilitySystem", this);
        this._blackboard = blackboard;
        IsStart = true;

        _manualStateMachine.InitStateMachine(_blackboard);

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
    /// <summary>
    /// 热更新能力列表，添加新的能力，移除不需要的能力（跨列表保留需重写Equals方法，确保能力唯一性）
    /// </summary>
    /// <param name="abilitityList"></param>
    public void SetAbilities(List<BaseAbility> abilitityList)
    {
        if (abilitityList == null)
            return;
        foreach (var ability in abilitityList)
        {
            if (abilities.Contains(ability))
                continue;
            _abilitiesToAdd.Add(ability);
        }
        foreach (var ability in abilities)
        {
            if (abilitityList.Contains(ability))
                continue;
            _abilitiesToRemove.Add(ability);
        }

        foreach (var ability in _abilitiesToRemove)
        {
            ability.OnRemove();
            abilities.Remove(ability);
        }
        foreach (var ability in _abilitiesToAdd)
        {
            abilities.Add(ability);
            ability.InitAbility(this, _playerInput, _blackboard);
        }
        _abilitiesToAdd.Clear();
        _abilitiesToRemove.Clear();
    }
}
