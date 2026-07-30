using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Space;
using Ray = Space.Ray;

public static class SpaceUtility
{
    /// <summary>
    /// 相交检测
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool IsIntersect(AABB a, AABB b)
    {
        return a.IsIntersect(b);
    }
    /// <summary>
    /// 相交检测
    /// </summary>
    /// <param name="ray"></param>
    /// <param name="aabb"></param>
    /// <returns></returns>
    public static bool IsIntersect(Ray ray, AABB aabb)
    {
        return ray.IntersectAABB(aabb, out _);
    }
    /// <summary>
    /// 相交检测
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="aabb"></param>
    /// <returns></returns>
    public static bool IsIntersect(Sphere sphere, AABB aabb)
    {
        return sphere.IntersectAABB(aabb);
    }
    /// <summary>
    /// 获取射线与AABB命中面的法线
    /// </summary>
    /// <param name="aabb">AABB包围盒</param>
    /// <param name="ray">射线</param>
    /// <returns>命中面法线（指向AABB外侧）；不相交返回Vector3.zero</returns>
    public static Vector3 GetNormal(AABB aabb, Ray ray)
    {
        ray.IntersectAABB(aabb, out _, out var normal);
        return normal;
    }

    /// <summary>
    /// 获取射线与AABB的命中点
    /// </summary>
    /// <param name="aabb">AABB包围盒</param>
    /// <param name="ray">射线</param>
    /// <returns>命中点世界坐标；不相交返回Vector3.zero</returns>
    public static Vector3 GetHitPosition(AABB aabb, Ray ray)
    {
        if (ray.IntersectAABB(aabb, out float distance, out _))
            return ray.origin + ray.dir * distance;
        return Vector3.zero;
    }

    /// <summary>
    /// 获取射线与AABB的命中信息（命中点、法线、距离）
    /// </summary>
    /// <param name="aabb">AABB包围盒</param>
    /// <param name="ray">射线</param>
    /// <param name="hitPoint">输出：命中点世界坐标</param>
    /// <param name="normal">输出：命中面的法线（指向AABB外侧）</param>
    /// <param name="distance">输出：射线起点到命中点的距离</param>
    /// <returns>true表示相交</returns>
    public static bool TryGetHitInfo(AABB aabb, Ray ray, out Vector3 hitPoint, out Vector3 normal, out float distance)
    {
        if (ray.IntersectAABB(aabb, out distance, out normal))
        {
            hitPoint = ray.origin + ray.dir * distance;
            return true;
        }
        hitPoint = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 获取球体与AABB碰撞的最近点和法线
    /// </summary>
    /// <param name="sphere">球体</param>
    /// <param name="aabb">AABB包围盒</param>
    /// <param name="closestPoint">输出：AABB上距离球心最近的点</param>
    /// <param name="normal">输出：碰撞法线（从球心指向最近点）</param>
    /// <returns>true表示相交</returns>
    public static bool TryGetHitInfo(Sphere sphere, AABB aabb, out Vector3 closestPoint, out Vector3 normal)
    {
        closestPoint = sphere.ClosestPoint(aabb);
        Vector3 diff = closestPoint - sphere.center;
        float sqrDist = diff.sqrMagnitude;
        if (sqrDist <= sphere.radius * sphere.radius)
        {
            if (sqrDist < 1e-7f)
            {
                // 球心在AABB内部（表面上），法线指向盒心方向
                normal = (aabb.center - sphere.center).normalized;
                if (normal == Vector3.zero)
                    normal = Vector3.up;
            }
            else
            {
                normal = diff.normalized;
            }
            return true;
        }
        normal = Vector3.zero;
        return false;
    }
}
