using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DynamicTextInfo
{
    public bool IsEffective => !text.Equals(string.Empty);
    public string text;
    public Color color;
    public Vector3 worldPoint;
    public DynamicTextInfo(string text, Color color, Vector3 point)
    {
        this.text = text ?? string.Empty;
        this.color = color;
        this.worldPoint = point;
    }
    public bool IsEffectiveWorldPosition(Camera camera)
    {
        if (camera == null)
            return false;
        var viewPos = camera.WorldToViewportPoint(worldPoint);
        if (viewPos.z < 0)
            return false;
        return viewPos.x >= 0 && viewPos.x <= 1
            && viewPos.y >= 0 && viewPos.y <= 1;
    }
}