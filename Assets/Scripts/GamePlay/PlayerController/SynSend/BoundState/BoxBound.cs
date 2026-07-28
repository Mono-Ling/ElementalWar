using System.Collections;
using System.Collections.Generic;
using Space;
using UnityEngine;

public class BoxBound : MonoBehaviour
{
    public AABB bound { get; private set; }
    public Vector3 offset;
    public Vector3 extents;
    public Color debugColor = Color.blue;
    void LateUpdate()
    => UpdateBound();
    void OnValidate()
    => UpdateBound();
    private void UpdateBound()
    => bound = new(transform.position + offset, extents);
    void OnDrawGizmosSelected()
    => bound.Draw(debugColor);
}
