using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    public string positionArgName = "Position";
    public string rotationArgName = "Rotation";
    public string animatorName = "Animator";
    public string pitchArgName = "Pitch";
    private Blackboard _blackboard;
    private Rigidbody _rigidbody;
    private void Start()
    {
        _blackboard = GetComponent<Blackboard>();
        if (_blackboard == null)
        {
            Debug.LogError("【PlayerMove】角色黑板获取失败");
            enabled = false;
            return;
        }
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            Debug.LogError("【PlayerMove】刚体获取失败");
            return;
        }
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Update is called once per frame
    void Update()
    {
        _blackboard.GetValue<Animator>(animatorName, out var animator);
        Vector3 v = _rigidbody.velocity;
        v = transform.InverseTransformDirection(v);
        animator?.SetFloat("MoveX", v.x);
        animator?.SetFloat("MoveY", v.z);
        animator?.SetFloat("MoveSpeed", v.magnitude);

        _blackboard.GetValue<float>(pitchArgName, out var pitch);
        animator?.SetFloat("AimY", pitch);
    }
    void FixedUpdate()
    {
        if (_blackboard.GetValue<Vector3>(positionArgName, out var pos))
            _rigidbody.MovePosition(pos);
        else
            Debug.LogError("【Player】位置设置失败");

        if (_blackboard.GetValue<Quaternion>(rotationArgName, out var rot))
            _rigidbody.MoveRotation(rot.normalized);
        else
            Debug.LogError("【Player】旋转设置失败");
    }
}
