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
            return IntersectAABB(aabb, out distance, out _);
        }

        /// <summary>
        /// 射线与AABB相交检测（Slab方法），同时返回命中点法线
        /// </summary>
        /// <param name="aabb">AABB包围盒</param>
        /// <param name="distance">输出：射线起点到最近交点的距离；若起点在包围盒内部则返回0</param>
        /// <param name="normal">输出：命中面的法线（指向AABB外侧）</param>
        /// <returns>true表示相交</returns>
        public bool IntersectAABB(AABB aabb, out float distance, out Vector3 normal)
        {
            Vector3 min = aabb.Min;
            Vector3 max = aabb.Max;

            float tMin = float.MinValue;
            float tMax = float.MaxValue;
            Vector3 entryNormal = Vector3.zero;
            Vector3 exitNormal = Vector3.zero;

            // X轴
            if (Mathf.Abs(dir.x) < EPSILON)
            {
                if (origin.x < min.x || origin.x > max.x)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.x;
                float t1 = (min.x - origin.x) * invDir;
                float t2 = (max.x - origin.x) * invDir;
                bool swapped = t1 > t2;
                if (swapped) { float tmp = t1; t1 = t2; t2 = tmp; }

                // 入口：未交换=命中min面(法线-X)，交换=命中max面(法线+X)
                if (t1 > tMin) { tMin = t1; entryNormal = swapped ? Vector3.right : Vector3.left; }
                // 出口：未交换=命中max面(法线+X)，交换=命中min面(法线-X)
                if (t2 < tMax) { tMax = t2; exitNormal = swapped ? Vector3.left : Vector3.right; }

                if (tMin > tMax)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }

            // Y轴
            if (Mathf.Abs(dir.y) < EPSILON)
            {
                if (origin.y < min.y || origin.y > max.y)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.y;
                float t1 = (min.y - origin.y) * invDir;
                float t2 = (max.y - origin.y) * invDir;
                bool swapped = t1 > t2;
                if (swapped) { float tmp = t1; t1 = t2; t2 = tmp; }

                if (t1 > tMin) { tMin = t1; entryNormal = swapped ? Vector3.up : Vector3.down; }
                if (t2 < tMax) { tMax = t2; exitNormal = swapped ? Vector3.down : Vector3.up; }

                if (tMin > tMax)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }

            // Z轴
            if (Mathf.Abs(dir.z) < EPSILON)
            {
                if (origin.z < min.z || origin.z > max.z)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }
            else
            {
                float invDir = 1f / dir.z;
                float t1 = (min.z - origin.z) * invDir;
                float t2 = (max.z - origin.z) * invDir;
                bool swapped = t1 > t2;
                if (swapped) { float tmp = t1; t1 = t2; t2 = tmp; }

                if (t1 > tMin) { tMin = t1; entryNormal = swapped ? Vector3.forward : Vector3.back; }
                if (t2 < tMax) { tMax = t2; exitNormal = swapped ? Vector3.back : Vector3.forward; }

                if (tMin > tMax)
                {
                    distance = 0f;
                    normal = Vector3.zero;
                    return false;
                }
            }

            // 射线完全在AABB后方
            if (tMax < 0f)
            {
                distance = 0f;
                normal = Vector3.zero;
                return false;
            }

            // tMin < 0 表示射线起点在包围盒内部，此时距离返回0，法线使用出口面
            if (tMin < 0f)
            {
                distance = 0f;
                normal = exitNormal;
            }
            else
            {
                distance = tMin;
                normal = entryNormal;
            }
            return true;
        }
    }
}
