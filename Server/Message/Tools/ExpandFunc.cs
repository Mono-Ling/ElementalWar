using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Message;

namespace Server.Message.Tools
{
    public static class ExpandFunc
    {
        public static void Switch(this Vector3Message message, Vector3 vector3)
        {
            message.X = vector3.X;
            message.Y = vector3.Y;
            message.Z = vector3.Z;
        }

        public static void Switch(this QuaternionMessage message, Quaternion quaternion)
        {
            message.X = quaternion.X;
            message.Y = quaternion.Y;
            message.Z = quaternion.Z;
            message.W = quaternion.W;
        }
        public static void Deconstruct(this Vector3Message message, out Vector3 vector,out object? _)
        {
            vector = new(message.X, message.Y, message.Z);
            _ = default;
        }
        public static void Deconstruct(this QuaternionMessage message, out Quaternion quaternion, out object? _)
        {
            quaternion = new(message.X, message.Y, message.Z,message.W);
            _ = default;
        }
    }
}
