using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAutoInject<T>
{
    void AutoInject(T inject);
}
