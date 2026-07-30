using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Space
{
    [Serializable]
    public struct AABB
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3 Max => center + extents;
        public Vector3 Min => center - extents;
        public float Volume => extents.x * extents.y * extents.z * 8;
        public AABB(Vector3 center, Vector3 extents)
        {
            this.center = center;
            this.extents = new(Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z));
        }
        /// <summary>
        /// 相交检测
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsIntersect(AABB other)
        {
            if (this.Min.x >= other.Max.x || other.Min.x >= this.Max.x)
                return false;
            if (this.Min.y >= other.Max.y || other.Min.y >= this.Max.y)
                return false;
            if (this.Min.z >= other.Max.z || other.Min.z >= this.Max.z)
                return false;
            return true;
        }
        /// <summary>
        /// 包含检测
        /// </summary>
        /// <param name="other">被包含AABB包围盒</param>
        /// <returns></returns>
        public bool IsContains(AABB other)
        {
            return this.Max.x >= other.Max.x && this.Min.x <= other.Min.x
                && this.Max.y >= other.Max.y && this.Min.y <= other.Min.y
                && this.Max.z >= other.Max.z && this.Min.z <= other.Min.z;
        }
        /// <summary>
        /// 点包含检测
        /// </summary>
        /// <param name="point">点坐标</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 point)
        {
            return point.x >= Min.x && point.x <= Max.x
                && point.y >= Min.y && point.y <= Max.y
                && point.z >= Min.z && point.z <= Max.z;
        }
        public static bool operator ==(AABB left, AABB right)
        {
            return left.center == right.center
                && left.extents == right.extents;
        }
        public static bool operator !=(AABB left, AABB right)
        {
            return !(left == right);
        }
        public bool Equals(AABB other) => center == other.center && extents == other.extents;
        public override bool Equals(object obj) => obj is AABB other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(center, extents);
        public void Draw(Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, extents * 2);
        }
    }
}
