using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extention
{
    /// <summary>
    /// returns true if the two types are the same 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="X"></typeparam>
    /// <param name="Type1"></param>
    /// <param name="type2"></param>
    /// <returns></returns>
    public static bool AreSameType<T, X>(T typeToCheck) => typeToCheck is X;

    
}
