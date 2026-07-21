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
            float invMag = 1f / Mathf.Sqrt(resultX * resultX + resultY * resultY +
                                           resultZ * resultZ + resultW * resultW);
            return new Quaternion(resultX * invMag, resultY * invMag,
                                  resultZ * invMag, resultW * invMag);
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
