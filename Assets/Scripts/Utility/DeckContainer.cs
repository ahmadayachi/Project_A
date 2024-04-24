using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "DeckContainer ")]
public class DeckContainer : ScriptableObject
{
    [Header("All Ranks on this Deck")]
   public List<CardSprite> SpriteContainer = new List<CardSprite>();
    public Sprite GetSuitSprite(byte Rank,byte Suit)
    {
        if(SpriteContainer.Count == 0)
            return null;
        Sprite suite;
        for (int index = 0; index < SpriteContainer.Count; index++)
        {
            if (SpriteContainer[index].Rank == Rank)
                return SpriteContainer[index].Suits[Suit];
        }
        return null;
    }
}
[Serializable]
public class CardSprite
{
    [Header("Card Rank")]
    public byte Rank;
    [Header("4 Suits for this rank in this Order:\r\n    Spades=0,\r\n    Diamonds=1,\r\n    Hearts=2,\r\n    Clover=3")]
    public Sprite[] Suits = new Sprite[4];
}
