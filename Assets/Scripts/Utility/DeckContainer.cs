using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "SpicyHarissa/DeckContainer ")]
public class DeckContainer : ScriptableObject
{
    [Header("All Ranks on this Deck")]
    public List<CardSprite> SpriteContainer = new List<CardSprite>();
    public Sprite GetSuitSprite(byte Rank, CardSuit Suit)
    {
        if (SpriteContainer.Count == 0 || Suit == CardSuit.NoSuit /*|| !Extention.IsAValidBeloteRank(Rank)*/)
        {
            return null;
        }
        Sprite suite;
        for (int index = 0; index < SpriteContainer.Count; index++)
        {
            if (SpriteContainer[index].Rank == Rank)
            {
                byte suitIndex = (byte)Suit;
                return SpriteContainer[index].Suits[suitIndex - 1];
            }
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
