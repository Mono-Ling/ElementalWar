using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void RefAction<T>(ref T arg);
public delegate void RefAction<T, W>(ref T arg1, ref W arg2);
public delegate void RefAction<T, W, U>(ref T arg1, ref W arg2, ref U arg3);
