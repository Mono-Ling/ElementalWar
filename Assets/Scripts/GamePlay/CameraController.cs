using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("目标")]
    public Transform followTarget;
    [Header("挂载点高度偏移")]
    public float h;
    public float length = 5f;
    public float pitch = 15;
    public float maxPitch = 50f;
    public float minPitch = -70f;
    void Start()
    {
        EventBus.Instance.AddListener<float>(EventType.CameraPitchDelta, SetPitch);
    }

    void LateUpdate()
    {
        if (followTarget == null) return;

        // 偏航角（Y轴）直接取角色的Y轴旋转，实现跟随竖直轴转动
        float yaw = followTarget.eulerAngles.y;

        // 合成相机的目标旋转：俯仰角 + 跟随的偏航角
        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 计算相机位置：角色位置 + 旋转后的偏移向量
        // 偏移：基础高度向上 + 沿角色后方向后退length
        Vector3 offset = new Vector3(0, h, -length);
        Vector3 targetPos = followTarget.position + targetRotation * offset;

        transform.position = targetPos;
        transform.rotation = targetRotation;
    }
    public void SetPitch(float delta)
    {
        pitch += delta;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        float pitchNum = -pitch;
        if (pitchNum < 0)
            pitchNum /= Mathf.Max(-minPitch, 0.01f);
        else
            pitchNum /= Mathf.Max(maxPitch, 0.01f);

        EventBus.Instance.Trigger(EventType.OnCameraPitchChange, pitchNum);
    }
    void OnDestroy()
    {
        EventBus.Instance.RemoveListener<float>(EventType.CameraPitchDelta, SetPitch);
    }
}
