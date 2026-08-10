using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnBuff : BaseElementBuff
{
    private float _delay;
    private float _speed;
    private float _preTime;
    public BurnBuff(float delay, float speed)
    {
        _delay = delay;
        _speed = speed;
    }
    public override bool TryExit()
    => !elementReceiver.TotalElement.HasFlag(ElementType.Grass);
    public override void OnUpdate()
    {
        if (Time.time - _preTime < _delay)
            return;
        _preTime = Time.time;

        elementReceiver.ReceiveElement(ElementType.Grass, _speed);
        Debug.Log("【燃烧Buff】燃烧伤害");
    }
}
