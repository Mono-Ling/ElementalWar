using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Space
{
    public struct Sphere
    {
        public Vector3 center;
        public float radius;

        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = Math.Abs(radius);
        }

        /// <summary>
        /// 球体包含点检测
        /// </summary>
        public bool ContainsPoint(Vector3 point)
        {
            return (point - center).LengthSquared() <= radius * radius;
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
            return (closest - center).LengthSquared() <= radius * radius;
        }

        /// <summary>
        /// 获取AABB上距离球心最近的点
        /// </summary>
        public Vector3 ClosestPoint(AABB aabb)
        {
            Vector3 min = aabb.Min;
            Vector3 max = aabb.Max;
            return new Vector3(
                Math.Clamp(center.X, min.X, max.X),
                Math.Clamp(center.Y, min.Y, max.Y),
                Math.Clamp(center.Z, min.Z, max.Z)
            );
        }
    }
}
