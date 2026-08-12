using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTextCreator : MonoBehaviour
{
    public Vector3 uiPostionOffset = Vector3.up;
    public float uiRadius = 0.2f;
    void Start()
    => DynamicTextManager.Instance.StartTextManager();
    void OnDestroy()
    => DynamicTextManager.Instance.StopTextManager();
    public void ShowTextUI(string text, Color color)
    {
        var angle = Random.Range(0, Mathf.PI + Mathf.PI);
        var radius = Random.Range(0, uiRadius);
        Vector3 pos = transform.position + uiPostionOffset;
        pos += new Vector3(Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius, 0);
        DynamicTextInfo info = new(text, color, pos);
        DynamicTextManager.Instance.LocalShowTextUI(info);
    }
}
