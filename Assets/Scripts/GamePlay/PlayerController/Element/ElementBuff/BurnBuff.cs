using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnBuff : BaseElementBuff
{
    public float delay;
    public float speed;
    public int damage = 10;
    private float _preTime;
    public override bool TryExit()
    => !elementAttachment.TotalElement.HasFlag(ElementType.Grass);
    public override void OnUpdate()
    {
        if (Time.time - _preTime < delay)
            return;
        _preTime = Time.time;

        elementReceiver.ReceiveElement(ElementType.Fire, speed, damage);
        Debug.Log("【燃烧Buff】燃烧伤害");
    }
}
