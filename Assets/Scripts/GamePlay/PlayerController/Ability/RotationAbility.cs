using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class RotationAbility : BaseAbility
{
    public float rotationSpeed;
    public float cameraRotationSpeed;
    private Rigidbody _rigidbody;
    private Vector2 _rotationInput;
    public override void InitAbility(MainPlayer mainPlayer, PlayerInput playerInput, Blackboard blackboard)
    {
        base.InitAbility(mainPlayer, playerInput, blackboard);
        _rigidbody = mainPlayer.GetComponent<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = mainPlayer.gameObject.AddComponent<Rigidbody>();

        AddInputPerformedListener("Rotation", OnRotationPerformed);
        AddInputCanceledListener("Rotation", OnRotationCanceled);
        EventBus.Instance.AddListener<float>(EventType.OnCameraPitchChange, OnCameraPitchChange);
    }
    private void OnRotationPerformed(InputAction.CallbackContext context)
    => _rotationInput = context.ReadValue<Vector2>();
    private void OnRotationCanceled(InputAction.CallbackContext context)
    => _rotationInput = Vector2.zero;
    public override void OnFixedUpdate()
    {
        Quaternion rotation = Quaternion.AngleAxis(_rotationInput.x * rotationSpeed * Time.fixedDeltaTime, Vector3.up);
        _rigidbody.MoveRotation(_rigidbody.rotation * rotation);
        blackboard.SetValue<Quaternion>("Rotation", _rigidbody.rotation);

        EventBus.Instance.Trigger(EventType.CameraPitchDelta, _rotationInput.y * cameraRotationSpeed * Time.fixedDeltaTime);
    }

    private void OnCameraPitchChange(float pitch)
    => blackboard.SetValue<float>("Pitch", pitch);

    public override void OnRemove()
    {
        EventBus.Instance.RemoveListener<float>(EventType.OnCameraPitchChange, OnCameraPitchChange);
        RemoveInputPerformedListener("Rotation", OnRotationPerformed);
        RemoveInputCanceledListener("Rotation", OnRotationCanceled);
    }
}
