using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class Math
    {
        /// <summary>
        /// 四元数平滑阻尼，算法参考 Vector3.SmoothDamp（临界阻尼弹簧模型）。
        /// currentVelocity 的四个分量分别对应四元数四个分量的变化速度。
        /// </summary>
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target,
            ref Quaternion currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity,
            float deltaTime = 0f)
        {
            if (deltaTime <= 0f)
                deltaTime = Time.deltaTime;

            if (smoothTime <= 0f || deltaTime <= 0f)
                return target;

            // 确保走最短弧：点积为负时翻转 target
            if (Quaternion.Dot(current, target) < 0f)
                NegateQuaternion(ref target);

            // 对四个分量分别应用 SmoothDamp
            float vx = currentVelocity.x, vy = currentVelocity.y,
                  vz = currentVelocity.z, vw = currentVelocity.w;

            float resultX = SmoothDampComponent(current.x, target.x, ref vx, smoothTime, maxSpeed, deltaTime);
            float resultY = SmoothDampComponent(current.y, target.y, ref vy, smoothTime, maxSpeed, deltaTime);
            float resultZ = SmoothDampComponent(current.z, target.z, ref vz, smoothTime, maxSpeed, deltaTime);
            float resultW = SmoothDampComponent(current.w, target.w, ref vw, smoothTime, maxSpeed, deltaTime);

            currentVelocity = new Quaternion(vx, vy, vz, vw);

            // 归一化保证结果仍是单位四元数
            float sqrMag = resultX * resultX + resultY * resultY +
                           resultZ * resultZ + resultW * resultW;

            // 防止模长过小导致 1/sqrt(≈0) 产生无穷大或 NaN
            const float minSqrMag = 1e-8f;
            if (sqrMag < minSqrMag)
            {
                // 退化情况：分量平滑后接近零向量，直接返回目标旋转
                return target;
            }

            float invMag = 1f / Mathf.Sqrt(sqrMag);
            return new Quaternion(resultX * invMag, resultY * invMag,
                                  resultZ * invMag, resultW * invMag);
        }
        /// <summary>
        /// 二阶贝塞尔曲线采样，非分配版本。points 数组长度为采样点数。
        /// 采样点包含起点和终点，采样点数至少为 2。
        /// </summary>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">终点</param>
        /// <param name="points"></param>
        public static void BezierCurveNonAlloc(Vector3 p0, Vector3 p1, Vector3 p2, Vector3[] points)
        {
            int sampleCount = points.Length;
            if (sampleCount <= 1)
                return;
            for (int i = 0; i < sampleCount; i++)
            {
                float s = (float)i / (sampleCount - 1);
                points[i] = (1f - s) * (1f - s) * p0 +
                            2f * (1f - s) * s * p1 +
                            s * s * p2;
            }
        }

        // 单分量临界阻尼平滑，与 Vector3.SmoothDamp 使用相同的 Game Programming Gems 4 公式
        private static float SmoothDampComponent(float current, float target,
            ref float velocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            float change = current - target;
            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);

            float temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;

            return target + (change + temp) * exp;
        }

        private static void NegateQuaternion(ref Quaternion q)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
    }
}
