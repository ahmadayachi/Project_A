using System;
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

    public static bool TryCastToStruct<T, X>(T structToCast, out X result) where X : struct
    {
        if (structToCast is X)
        {
            result = (X)(object)structToCast;
            return true;
        }
        else
        {
            //default is null 
            result = default;
            return false;
        }
    }


}
