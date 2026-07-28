using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Space
{
    public struct Ray
    {
        public Vector3 origin;
        public Vector3 dir;

        private const float EPSILON = 1e-7f;

        public Ray(Vector3 origin, Vector3 dir)
        {
            this.origin = origin;
            this.dir = Vector3.Normalize(dir);
        }

        /// <summary>
        /// 射线与AABB相交检测（Slab方法）
        /// </summary>
        /// <param name="aabb">AABB包围盒</param>
        /// <param name="distance">输出：射线起点到最近交点的距离；若起点在包围盒内部则返回0</param>
        /// <returns>true表示相交</returns>
        public bool IntersectAABB(AABB aabb, out float distance)
        {
            Vector3 min = aabb.Min;
            Vector3 max = aabb.Max;

            float tMin = float.MinValue;
            float tMax = float.MaxValue;

            // X轴
            if (Math.Abs(dir.X) < EPSILON)
            {
                if (origin.X < min.X || origin.X > max.X)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.X;
                float t1 = (min.X - origin.X) * invDir;
                float t2 = (max.X - origin.X) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax)
                {
                    distance = 0f;
                    return false;
                }
            }

            // Y轴
            if (Math.Abs(dir.Y) < EPSILON)
            {
                if (origin.Y < min.Y || origin.Y > max.Y)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.Y;
                float t1 = (min.Y - origin.Y) * invDir;
                float t2 = (max.Y - origin.Y) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax)
                {
                    distance = 0f;
                    return false;
                }
            }

            // Z轴
            if (Math.Abs(dir.Z) < EPSILON)
            {
                if (origin.Z < min.Z || origin.Z > max.Z)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.Z;
                float t1 = (min.Z - origin.Z) * invDir;
                float t2 = (max.Z - origin.Z) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);
                if (tMin > tMax)
                {
                    distance = 0f;
                    return false;
                }
            }

            // 射线完全在AABB后方
            if (tMax < 0f)
            {
                distance = 0f;
                return false;
            }

            // tMin < 0 表示射线起点在包围盒内部
            distance = tMin >= 0f ? tMin : 0f;
            return true;
        }
    }
}
