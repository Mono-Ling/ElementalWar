using System.Collections;
using System.Collections.Generic;
using Space;

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
}
