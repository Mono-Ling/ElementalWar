using System.Collections;
using System.Collections.Generic;
using Space;
using UnityEngine;

public class StaticSceneItem : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 extents;
    public StaticSceneInfo info { get; private set; } = new();
    public Color debugColor = Color.green;
    private bool _isUpdate = true;
    void Update()
    {
        if (_isUpdate)
            UpdateBound();
    }
    void OnValidate()
    => UpdateBound();
    private void UpdateBound()
    {
        info.bound = new(transform.position + offset, extents);
        info.position = transform.position;
    }
    void OnDrawGizmosSelected()
    => info.bound.Draw(debugColor);
    public void SetInfo(StaticSceneInfo info)
    {
        this.info = info;
        transform.position = info.position;
        _isUpdate = false;
    }
}
