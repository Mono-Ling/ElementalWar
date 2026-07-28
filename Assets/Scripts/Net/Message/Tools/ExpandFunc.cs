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
}
