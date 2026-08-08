using System.Collections;
using System.Collections.Generic;
using Space;
using UnityEngine;

public class StaticSceneItem : MonoBehaviour
{
    private const string HIT_EFF_PATH = "HitEff";
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
        info.scale = transform.localScale;
    }
    void OnDrawGizmosSelected()
    => info.bound.Draw(debugColor);
    public void SetInfo(StaticSceneInfo info)
    {
        this.info = info;
        transform.position = info.position;
        transform.localScale = info.scale;
        _isUpdate = false;
    }
    public void OnHit(Space.Ray ray, ElementType elementType)
    {
        Debug.Log("【命中】", gameObject);
        Vector3 hitPos = SpaceUtility.GetHitPosition(info.bound, ray);
        Vector3 hitNor = SpaceUtility.GetNormal(info.bound, ray);

        var obj = MonoObjectPool.Instance.GetObject(HIT_EFF_PATH);
        if (obj == null)
        {
            Debug.LogError("【StaticSceneItem】枪击特效加载失败", gameObject);
            return;
        }
        var eff = obj.GetComponent<HitEff>();
        if (eff != null)
            eff.ShowEff(hitPos, hitNor, elementType);
    }
}
