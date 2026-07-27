using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            this.dir = dir.normalized;
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
            if (Mathf.Abs(dir.x) < EPSILON)
            {
                if (origin.x < min.x || origin.x > max.x)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.x;
                float t1 = (min.x - origin.x) * invDir;
                float t2 = (max.x - origin.x) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);
                if (tMin > tMax)
                {
                    distance = 0f;
                    return false;
                }
            }

            // Y轴
            if (Mathf.Abs(dir.y) < EPSILON)
            {
                if (origin.y < min.y || origin.y > max.y)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.y;
                float t1 = (min.y - origin.y) * invDir;
                float t2 = (max.y - origin.y) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);
                if (tMin > tMax)
                {
                    distance = 0f;
                    return false;
                }
            }

            // Z轴
            if (Mathf.Abs(dir.z) < EPSILON)
            {
                if (origin.z < min.z || origin.z > max.z)
                {
                    distance = 0f;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.z;
                float t1 = (min.z - origin.z) * invDir;
                float t2 = (max.z - origin.z) * invDir;
                if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);
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
