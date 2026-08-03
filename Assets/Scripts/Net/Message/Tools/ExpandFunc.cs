using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;

public static class ExpandFunc
{
    public static void Switch(this Vector3Message message, Vector3 vector3)
    {
        message.X = vector3.x;
        message.Y = vector3.y;
        message.Z = vector3.z;
    }

    public static void Switch(this QuaternionMessage message, Quaternion quaternion)
    {
        message.X = quaternion.x;
        message.Y = quaternion.y;
        message.Z = quaternion.z;
        message.W = quaternion.w;
    }
    public static void Switch(this ColorMessage message, Color color)
    {
        message.R = color.r;
        message.G = color.g;
        message.B = color.b;
        message.A = color.a;
    }
    public static void Deconstruct(this Vector3Message message, out Vector3 vector, out object _)
    {
        vector = new(message.X, message.Y, message.Z);
        _ = default;
    }
    public static void Deconstruct(this QuaternionMessage message, out Quaternion quaternion, out object _)
    {
        quaternion = new(message.X, message.Y, message.Z, message.W);
        _ = default;
    }
    public static void Deconstruct(this ColorMessage message, out Color color, out object _)
    {
        color = new(message.R, message.G, message.B, message.A);
        _ = default;
    }
}
