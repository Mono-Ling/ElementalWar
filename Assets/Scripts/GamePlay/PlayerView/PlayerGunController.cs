using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunController : MonoBehaviour
{
    public Transform gun;
    public Transform leftHandPos;
    public Transform rightHandPos;
    public Transform firedPos;
    [Range(0, 1)]
    [SerializeField]
    private float _backwardProgress;
    private Blackboard _blackboard;
    private Vector3 _gunInitPos;
    // Start is called before the first frame update
    void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
            Debug.LogError("【角色枪械控制器】黑板获取失败");

        if (gun == null)
            Debug.LogError("【角色枪械控制器】枪械标记点为空");
        if (leftHandPos == null || rightHandPos == null)
            Debug.LogError("【角色枪械控制器】手部对齐点为空");
        if (firedPos == null)
            Debug.LogError("【角色枪械控制器】开火后座点为空");
        _gunInitPos = gun.localPosition;
    }
    void Update()
    {
        _blackboard.GetValue<float>("FireProgress", out var progress);
        _backwardProgress = progress;
        gun.transform.localPosition = Vector3.Lerp(_gunInitPos, firedPos.localPosition, _backwardProgress);
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (!_blackboard.GetValue<Animator>("Animator", out var animator) ||
            animator == null)
        {
            Debug.LogError("【角色枪械控制器】动画组件获取失败");
            return;
        }
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandPos.rotation);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandPos.rotation);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
    }
}
