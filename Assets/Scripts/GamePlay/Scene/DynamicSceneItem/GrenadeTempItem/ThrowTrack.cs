using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThrowTrack : MonoBehaviour
{
    private const int SAMPLE_COUNT = 30;
    public float width = 0.1f;
    public LayerMask layerMask;
    public Material lineMaterial;
    private LineRenderer _lineRenderer;
    private Vector3[] _points = new Vector3[SAMPLE_COUNT];

    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null)
        {
            Debug.LogError("【投掷轨迹】线渲染组件获取失败");
            return;
        }
        if (lineMaterial == null)
            Debug.LogError("【投掷轨迹】线渲染材质为空");
        _lineRenderer.startWidth = width;
        _lineRenderer.endWidth = width;
        _lineRenderer.material = lineMaterial;
    }
    public void UpdateTrack(Vector3 throwDir, float throwForce, Vector3 startPos)
    {
        Vector3 p0 = startPos;
        Vector3 v0 = throwDir * throwForce;
        Vector3 g = Physics.gravity;
        float T = GetTotalTime(p0, v0, g);
        if (T <= 0)
            return;
        Vector3 p2 = p0 + v0 * T + 0.5f * g * T * T;
        Vector3 p1 = p0 + v0 * (T * 0.5f);

        Tools.Math.BezierCurveNonAlloc(p0, p1, p2, _points);

        Vector3 prePoint = _points[0];
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, prePoint);
        for (int i = 1; i < SAMPLE_COUNT; i++)
        {
            Vector3 currPoint = _points[i];
            _lineRenderer.positionCount = i + 1;
            if (Physics.Linecast(prePoint, currPoint, out var hit, layerMask))
            {
                _lineRenderer.SetPosition(i, hit.point);
                return;
            }
            _lineRenderer.SetPosition(i, currPoint);
            prePoint = currPoint;
        }
    }
    private float GetTotalTime(Vector3 p0, Vector3 v0, Vector3 g)
    {
        // a * T ^ 2 + b * T + c = 0
        float a = 0.5f * g.y;
        float b = v0.y;
        float c = p0.y;

        float delta = b * b - 4 * a * c;
        if (delta < 0)
        {
            Debug.LogError("【投掷轨迹】抛物线计算失败, 无法计算抛物运动时间");
            return default;
        }
        float T = (-b + Mathf.Sqrt(delta)) / (2 * a);
        if (T <= 0)
            T = (-b - Mathf.Sqrt(delta)) / (2 * a);
        return T;
    }
}
