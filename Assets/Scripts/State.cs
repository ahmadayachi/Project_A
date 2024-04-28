using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State 
{
    public abstract  void Start<T>(T arg) where T : struct;
    public abstract void ForceEnd();
}
