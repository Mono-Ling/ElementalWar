using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string animatorName = "Animator";
    public Blackboard blackboard { get; private set; }
    // Start is called before the first frame update
    void Awake()
    {
        blackboard = GetComponent<Blackboard>();
        if (blackboard == null)
        {
            Debug.LogError("【玩家控制器】玩家黑板为空");
            return;
        }
        Animator animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("【角色控制器】动画组件获取失败");
        blackboard.SetValue<Animator>(animatorName, animator);
    }
}
