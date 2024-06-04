using System;
using System.Linq;
using UnityEngine;

#if OldCardManager

public struct CardStaticStructure
{
    public int Name;
    public CardSuite Type;

    public override string ToString()
    {
        return $"CardStaticStructure Type: {Type} Name {Name}";
    }
}
#endif

public static class CardManager
{
#if OldCardManager
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
#endif
    public const byte BELOTE_DECK_SUIT_SIZE = 8;
    public const byte STANDARD_DECK_SUIT_SIZE = 13;
    private static CardInfo[] _cards;
    private static byte[] _sortedRanks;

    /// <summary>
    /// Standart Belote Deck
    /// </summary>
    public static CardInfo[] Deck { get => _cards; }

    public static byte[] SortedRanks { get => _sortedRanks; }

    private static byte _maxRankCounter;

    /// <summary>
    /// represents the total number each rank from all suits in Deck
    /// </summary>
    public static byte MaxRankCounter { get => _maxRankCounter; }

    public static CardInfo GetCard(byte ID)
    {
        if (ID <= 0)
        {
#if Log
            LogManager.LogError($"the ID Provided is Not Valid {ID}");
#endif
            //handle null cards outside
            return default;
        }
        return _cards[ID - 1];
    }

    public static void Init(DeckInfo deckInfo)
    {
        CreateDeck(deckInfo);
    }

    public static void Reset()
    {
        Array.Clear(_cards, 0, _cards.Length);
    }

    //Create Deck Dynamically
    private static void CreateDeck(DeckInfo deckInfo)
    {
        //blocking is suits number is not enough
        if (deckInfo.SuitsNumber < 1)
        {
#if Log
            LogManager.LogError($"{deckInfo.SuitsNumber} this Suit number is too low to create a deck!");
#endif
            return;
        }
        //setting up rank counter
        _maxRankCounter = deckInfo.SuitsNumber;

        //setting up deck size
        int deckSize = SetCardsArraySize(deckInfo.DeckType,
                                         deckInfo.SuitsNumber,
                                         deckInfo.CustomSuitRanks == null ? 0 : deckInfo.CustomSuitRanks.Length);
        if (deckSize <= 0)
        {
#if Log
            LogManager.LogError($"Invalid Deck Size ! deckSize={deckSize}");
#endif
            return;
        }
        _cards = new CardInfo[deckSize];

        //setting up starting index of the loop and maxIterations
        byte startingRankIndex;
        byte maxIterations;

        if (deckInfo.CustomSuitRanks == null)
        {
            startingRankIndex = (byte)deckInfo.DeckType;
            maxIterations = (STANDARD_DECK_SUIT_SIZE + 1);
        }
        else
        {
            startingRankIndex = 0;
            maxIterations = (byte)deckInfo.CustomSuitRanks.Length;
        }

        //filling the cards array
        byte cardsDeckIndex = 0;
        CardInfo card;
        byte cardID = 1;
        byte ace = 1;
        byte cardRank = 0;

        for (byte suitIndex = 1; suitIndex < (deckInfo.SuitsNumber + 1); suitIndex++)
        {
            //adding the suit Ranks
            for (byte rankIndex = startingRankIndex; rankIndex < maxIterations; rankIndex++)
            {
                cardRank = deckInfo.CustomSuitRanks == null ? rankIndex : deckInfo.CustomSuitRanks[rankIndex];
                card = new CardInfo()
                {
                    Rank = cardRank,
                    ID = cardID++,
                    Suit = Extention.ByteToCardSuit(suitIndex),
                    IsValid = true
                };

                _cards[cardsDeckIndex++] = card;
            }
            // adding Ace at the end of a suit (only needed when it is not a custom deck)
            if (deckInfo.CustomSuitRanks == null)
            {
                card = new CardInfo()
                {
                    Rank = ace,
                    ID = cardID++,
                    Suit = Extention.ByteToCardSuit(suitIndex),
                    IsValid = true
                };
                _cards[cardsDeckIndex++] = card;
            }
        }
        //filling sorted ranks array
        SetUpSortedRanks(deckInfo);
    }

    private static int SetCardsArraySize(DeckType deckType, byte SuitsNumber, int CustomArrayLengh)
    {
        int deckSize = 0;
        switch (deckType)
        {
            case DeckType.Standard: deckSize = (SuitsNumber * STANDARD_DECK_SUIT_SIZE); break;
            case DeckType.Belote: deckSize = (SuitsNumber * BELOTE_DECK_SUIT_SIZE); break;
            case DeckType.Custom: deckSize = (SuitsNumber * CustomArrayLengh); break;
        }
        return deckSize;
    }

    private static void SetUpSortedRanks(DeckInfo DeckInfo)
    {
        //for now some are hard coded
        switch (DeckInfo.DeckType)
        {
            case DeckType.Standard: _sortedRanks = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }; break;
            case DeckType.Belote: _sortedRanks = new byte[] { 7, 8, 9, 11, 12, 13, 10, 1 }; break;
            case DeckType.Custom:
                {
                    if (DeckInfo.CustomSuitRanks == null)
                    {
#if Log
                        LogManager.LogError("failed Creating a Sorted Ranks Array for a Custom Deck!");
#endif
                        return;
                    }
                    _sortedRanks = new byte[DeckInfo.CustomSuitRanks.Length];
                    Array.Copy(DeckInfo.CustomSuitRanks, _sortedRanks, DeckInfo.CustomSuitRanks.Length);
                }; break;
        }
    }
}