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
    #region Array extentions
    /// <summary>
    /// Fisher-Yates shuffle 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public static void Shuffle<T>(this T[] array)
    {
        System.Random rng = new System.Random();
        int length = array.Length-1;
        for (int index = length; index > 0; index--)
        {
            int RandomIndex = rng.Next(index + 1);
            T temp = array[index];
            array[index] = array[RandomIndex];
            array[RandomIndex] = temp;
        }
    }
    public static void AddCard(this CardInfo[] array, CardInfo card)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (!array[index].IsValid)
            {
                array[index] = card;
                return;
            }
        }
#if Log
        Debug.LogError("No available spot in the array.");
#endif
    }

    #endregion
}
