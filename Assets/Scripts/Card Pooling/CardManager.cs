using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CardStaticStructure
{
    public int Name;
    public CardSuite Type;

    public override string ToString()
    {
        return $"CardStaticStructure Type: {Type} Name {Name}";
    }
}



public static class CardManager
{
    public const int DEFAULT_DECK_SIZE = 52;
    public const int STANDARD_DECK_CARD_NUMBER = 13;
    public const int CARD_TYPE_NUMBER = 4;

    private static CardStaticStructure[] cards = new CardStaticStructure[DEFAULT_DECK_SIZE];
    private static CardSuite lastChoosenType = default;

    // public CardStaticStructure this[int ID]
    // {
    //     get
    //     {
    //         return cards[ID];
    //     }
    // }


    public static CardStaticStructure GetCard(int ID)
    {
        return cards[ID];
    }

    // Start is called before the first frame update
    public static void Init()
    {
        PopulateArray();
    }

    //TODO: Extend as 13 segement
    /// <summary>
    /// Use extend to double the capacity of the array this is useful when we want 104 card which is basicaily something like 5 to 8 players
    /// <para>Passing in 0 will return </para>
    /// </summary>
    public static void ExtendArray(int numberOfTimes=1)
    {
        if (numberOfTimes == 0)
        {
            Debug.LogError("Invalid parameter of 0");
            return;
        }
        int newLength = cards.Length + STANDARD_DECK_CARD_NUMBER * numberOfTimes;
        CardStaticStructure[] _newArray = new CardStaticStructure[newLength];
        int i = 0;
        for (i = 0; i < cards.Length; i++)
        {
            _newArray[i] = cards[i];
        }

        cards = _newArray;

        int arrayIndex = i;
        for (int d= 0; d < numberOfTimes; d++)
        {
            for (int j = 0; j < STANDARD_DECK_CARD_NUMBER; j++)
            {
                cards[arrayIndex].Name = j;
                cards[arrayIndex].Type = lastChoosenType;
                arrayIndex++;
            }
            lastChoosenType++;
        }
     

 

        //unsure but should ask for clean up specifically previous array
        GC.Collect();
    }

    private static void PopulateArray(int startIndex = 0)
    {
        Debug.Log($"PopulateArray at index {startIndex}");
        int arrayIndex = startIndex;
        for (int i = 0; i < STANDARD_DECK_CARD_NUMBER; i++)
        {
            for (int j = 0; j < CARD_TYPE_NUMBER; j++)
            {
                cards[arrayIndex].Name = i;
                cards[arrayIndex].Type = (CardSuite)j;
                arrayIndex++;
            }
        }
    }
}