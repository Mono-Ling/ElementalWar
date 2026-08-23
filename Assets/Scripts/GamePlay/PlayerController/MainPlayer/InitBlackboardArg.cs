using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitBlackboardArg : MonoBehaviour
{
    public int maxBulletCount = 30;
    public int maxGrenadeCount = 15;
    public ElementType defaultElement = ElementType.Fire;
    public void InitArg(Blackboard blackboard)
    {
        if (blackboard == null)
        {
            Debug.LogError("【黑板参数初始化】黑板为空初始化失败");
            return;
        }
        blackboard.SetValue("BulletCount", maxBulletCount);
        blackboard.SetValue("MaxBulletCount", maxBulletCount);

        blackboard.SetValue("GrenadeCount", maxGrenadeCount);

        blackboard.SetValue("AttackElementType", defaultElement);
        blackboard.SetValue("ShootElementType", defaultElement);
    }
}
