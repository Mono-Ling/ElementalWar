using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Space
{
    public struct Sphere
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = Mathf.Abs(radius);
        }

        /// <summary>
        /// 球体包含点检测
        /// </summary>
        public bool ContainsPoint(Vector3 point)
        {
            return (point - center).sqrMagnitude <= radius * radius;
        }

        /// <summary>
        /// 球体与AABB相交检测（最近点法）
        /// </summary>
        /// <param name="aabb">AABB包围盒</param>
        /// <returns>true表示相交</returns>
        public bool IntersectAABB(AABB aabb)
        {
            // 找到AABB上距离球心最近的点
            Vector3 closest = ClosestPoint(aabb);
            return (closest - center).sqrMagnitude <= radius * radius;
        }

        /// <summary>
        /// 获取AABB上距离球心最近的点
        /// </summary>
        public Vector3 ClosestPoint(AABB aabb)
        {
            Vector3 min = aabb.Min;
            Vector3 max = aabb.Max;
            return new Vector3(
                Mathf.Clamp(center.x, min.x, max.x),
                Mathf.Clamp(center.y, min.y, max.y),
                Mathf.Clamp(center.z, min.z, max.z)
            );
        }
    }
}
