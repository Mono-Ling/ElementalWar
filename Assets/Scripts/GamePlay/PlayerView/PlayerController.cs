using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string animatorName = "Animator";
    private Blackboard _blackboard;
    // Start is called before the first frame update
    void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【玩家控制器】玩家黑板为空");
            return;
        }
        Animator animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("【角色控制器】动画组件获取失败");
        _blackboard.SetValue<Animator>(animatorName, animator);
    }
    public void SetPosition(Vector3 pos) => _blackboard.SetValue("Position", pos);
    public void SetRotation(Quaternion rot) => _blackboard.SetValue("Rotation", rot);
    public void SetPitch(float pitch) => _blackboard.SetValue("Pitch", pitch);
}
