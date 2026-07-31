using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicMono : SingleMono<PublicMono>
{
    public event Action OnUpdate;
    public event Action OnLateUpdate;
    public event Action OnFixedUpdate;
    void Update()
    => OnUpdate?.Invoke();
    void LateUpdate()
    => OnLateUpdate?.Invoke();
    void FixedUpdate()
    => OnFixedUpdate?.Invoke();
}
