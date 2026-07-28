using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Space
{
    public struct AABB
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3 Max => center + extents;
        public Vector3 Min => center - extents;
        public float Volume => extents.X * extents.Y * extents.Z * 8;
        public AABB(Vector3 center, Vector3 extents)
        {
            this.center = center;
            this.extents = new(Math.Abs(extents.X), Math.Abs(extents.Y), Math.Abs(extents.Z));
        }
        /// <summary>
        /// 相交检测
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsIntersect(AABB other)
        {
            if (this.Min.X >= other.Max.X || other.Min.X >= this.Max.X)
                return false;
            if (this.Min.Y >= other.Max.Y || other.Min.Y >= this.Max.Y)
                return false;
            if (this.Min.Z >= other.Max.Z || other.Min.Z >= this.Max.Z)
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
            return this.Max.X >= other.Max.X && this.Min.X <= other.Min.X
                && this.Max.Y >= other.Max.Y && this.Min.Y <= other.Min.Y
                && this.Max.Z >= other.Max.Z && this.Min.Z <= other.Min.Z;
        }
        /// <summary>
        /// 点包含检测
        /// </summary>
        /// <param name="point">点坐标</param>
        /// <returns></returns>
        public bool ContainsPoint(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X
                && point.Y >= Min.Y && point.Y <= Max.Y
                && point.Z >= Min.Z && point.Z <= Max.Z;
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
        public override bool Equals(object? obj) => obj is AABB other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(center, extents);
    }
}
