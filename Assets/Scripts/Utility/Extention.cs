using Codice.Client.BaseCommands.BranchExplorer;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Extention
{
    #region Generic shit

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

    public static bool IsAValidBeloteRank(byte rank)
    {
        if (rank == 1) return true;
        if (rank >= 7 && rank <= 13) return true;
        return false;
    }

    /// <summary>
    /// Cast any byte to Card Suit Enum
    /// </summary>
    /// <param name="suitNumber"></param>
    /// <returns></returns>
    public static CardSuit ByteToCardSuit(byte suitNumber)
    {
        if (suitNumber == 0)
        {
            return CardSuit.NoSuit;
        }
        else
        {
            var Suit = (((suitNumber - 1) % 4) + 1);
            return (CardSuit)Suit;
        }
    }

    #endregion Generic shit

    #region Cards Array extentions

    /// <summary>
    /// Fisher-Yates shuffle
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public static void Shuffle<T>(this T[] array)
    {
        System.Random rng = new System.Random();
        int length = array.Length - 1;
        for (int index = length; index > 0; index--)
        {
            int RandomIndex = rng.Next(index + 1);
            T temp = array[index];
            array[index] = array[RandomIndex];
            array[RandomIndex] = temp;
        }
    }

    public static bool AddCard(this CardInfo[] array, CardInfo card)
    {
        if (!card.IsValid || array.Contains(card))
            return false;
        bool CardIsAdded = false;
        for (int index = 0; index < array.Length; index++)
        {
            if (!array[index].IsValid)
            {
                array[index] = card;
                CardIsAdded = true;
                break;
            }
        }
        return CardIsAdded;
    }

    /// <summary>
    /// If Cards Are Same (returns True ) , (Returns False ) if either Cards are not valid
    /// </summary>
    /// <param name="firstCard"></param>
    /// <param name="secondCard"></param>
    /// <returns></returns>
    public static bool AreSameCard(CardInfo firstCard, CardInfo secondCard)
    {
        if (!firstCard.IsValid || !secondCard.IsValid)
            return false;

        return (firstCard.ID == secondCard.ID &&
                firstCard.Rank == secondCard.Rank &&
                firstCard.Suit == secondCard.Suit);
    }

    public static bool RemoveCard(this CardInfo[] array, CardInfo cardToRemove)
    {
        //blocking if card is not instentiated
        if (cardToRemove.IsValid) return false;

        bool IsCardRemoved = false;

        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], cardToRemove))
            {
                array[index] = new CardInfo();
                IsCardRemoved = true;
                break;
            }
        }
        return IsCardRemoved;
    }

    /// <summary>
    /// true if the array is null or size (arrray length is 0)
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsNotInitialized(this CardInfo[] array)
    {
        return (array == null || array.Length == 0);
    }

    /// <summary>
    /// the total Number of Cards that are Valid In this Array
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static int ValidCardsCount(this CardInfo[] array)
    {
        if (array.IsNotInitialized()) return 0;

        int count = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index].IsValid))
                count++;
        }
        return count;
    }

    /// <summary>
    /// true if Valid cards Count is 0
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool IsEmpty(this CardInfo[] array)
    {
        return array.ValidCardsCount() == 0;
    }

    public static bool ContainsCard(this CardInfo[] array, CardInfo card)
    {
        if (array.IsNotInitialized())
        {
#if Log
            LogManager.LogError("Array is Not Intilialized !");
#endif
            return false;
        }
        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], card))
            { return true; }
        }
        return false;
    }

    public static bool ContainsRank(this CardInfo[] array, byte rank)
    {
        if (array.IsNotInitialized()) return false;
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].Rank == rank)
            { return true; }
        }
        return false;
    }

    public static int DuplicateCounter(this CardInfo[] array, byte rank)
    {
        byte counter = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].Rank == rank)
                counter++;
        }
        return counter;
    }

    #endregion Cards Array extentions

    #region Networked Card Array extentions

    public static int ValidCardsCount(this NetworkArray<byte> array)
    {
        int count = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index] != 0))
                count++;
        }
        return count;
    }

    public static bool IsEmpty(this NetworkArray<byte> array)
    {
        return array.ValidCardsCount() == 0;
    }

    public static CardInfo[] ToCardInfo(this NetworkArray<byte> array)
    {
        if (CardManager.Deck == null)
        {
#if Log
            Debug.LogError($"Failed Converting Neworked  Cards Array! Cardmanager Deck is Null!");
#endif
            return null;
        }
        CardInfo[] cards = null;
        if (array.IsEmpty())
        {
            return null;
        }
        else
        {
            cards = new CardInfo[array.ValidCardsCount()];
            for (int index = 0; index < array.Length; index++)
            {
                var cardID = array[index];
                if (cardID != 0)
                {
                    var card = CardManager.GetCard(cardID);
                    cards[index] = card;
                }
            }
        }
        return cards;
    }

    public static void Clear(this NetworkArray<CardInfo> array)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index].IsValid)
                array[index] = new CardInfo();
        }
    }

    public static bool AddCardID(this NetworkArray<byte> array, CardInfo card)
    {
        if (!card.IsValid || card.ID == 0)
        {
#if Log
            LogManager.LogError("Attemp to add invalid Card to Array!");
#endif
            return false;
        }
        if (array.ContainsCardID(card.ID))
        {
#if Log
            LogManager.LogError("array Already Contains Card!");
#endif
            return false;
        }
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index] == 0)
            {
                array[index] = card.ID;
                return true;
            }
        }
        return false;
    }

    public static bool ContainsCard(this NetworkArray<CardInfo> array, CardInfo card)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (AreSameCard(array[index], card))
                return true;
        }
        return false;
    }

    public static bool ContainsCardID(this NetworkArray<byte> array, byte cardID)
    {
        for (int index = 0; index < array.Length; index++)
        {
            if (array[index] == cardID)
                return true;
        }
        return false;
    }

    #endregion Networked Card Array extentions

    #region UI

    /// <summary>
    /// sets a giving child to a giving parent
    /// </summary>
    /// <param name="_transform"></param>
    /// <param name="Parent"></param>
    public static void SetParent(Transform _transform, Transform Parent)
    {
        _transform.SetParent(Parent, false);
    }

    public static void FindCanvasAndSetLastSibling(Transform transform)
    {
        Canvas canvasgo = MonoBehaviour.FindObjectOfType<Canvas>();
        if (canvasgo != null)
        {
            SetParent(transform, canvasgo.transform);
            transform.SetAsLastSibling();
        }
#if Log
        else
            LogManager.LogError("No GameObject with the Name Canvas Have Been Found !");
#endif
    }

    #endregion UI

    #region byte Array

    public static int ValidCardsCount(this byte[] array)
    {
        int count = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index] != 0))
                count++;
        }
        return count;
    }

    public static bool IsEmpty(this byte[] array)
    {
        return array.ValidCardsCount() == 0;
    }

    public static bool IsNullOrEmpty(this byte[] array)
    {
        return (array.IsEmpty() || array == null);
    }

    public static bool TryGetRankValue(this byte[] array, byte rank, out int Value)
    {
        Value = 0;
        for (int index = 0; index < array.Length; ++index)
        {
            if (array[index] == rank)
            {
                Value = index;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Epic Method to get Rank Value , if rank value found it returns true
    /// </summary>
    /// <param name="array"></param>
    /// <param name="Rank"></param>
    /// <param name="Index"></param>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static bool TryGetRankBruteValueAlpha(this byte[] array, byte Rank, int Index, out int Value)
    {
        Value = 0;
        if (Index >= array.Length) return false;
        if (array[Index] == Rank)
        {
            Value = Index;
            return true;
        }
        return TryGetRankBruteValueAlpha(array, Rank, ++Index, out Value);
    }

    public static bool IsRankDiffused(this Dictionary<byte, byte> bet, byte rank)
    {
        if (bet.Count == 0) return false;
        foreach (byte betRank in bet.Keys)
        {
            if (betRank == rank)
                return true;
        }
        return false;
    }

    public static int RankCounter(byte[] bet, byte rank)
    {
        int counter = 0;
        foreach (var item in bet)
        {
            if (item == rank)
                counter++;
        }
        return counter;
    }

    public static int RankCounterAlpha(byte[] Bet, byte rank, int Index, int Counter)
    {
        if (Index >= Bet.Length) return Counter;
        if (Bet[Index] == rank) return RankCounterAlpha(Bet, rank, ++Index, ++Counter);
        return RankCounterAlpha(Bet, rank, ++Index, Counter);
    }

    public static void BetDiffuser(byte[] bet, Dictionary<byte, byte> diffusedBet)
    {
        foreach (byte rank in bet)
        {
            if (diffusedBet.IsRankDiffused(rank))
                continue;
            else
                diffusedBet.Add(rank, (byte)RankCounterAlpha(bet, rank, 0, 0));
        }

        //TODO move to sorting method ,
        // returning if there is only one set of ranks
        if (diffusedBet.Count == 1) return;

        //first sorting with Card Count
        Dictionary<byte, byte> cardsCounterSortedVessel = diffusedBet.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        //copying shit back
        diffusedBet.Clear();
        foreach (var rankPair in cardsCounterSortedVessel)
            diffusedBet.Add(rankPair.Key, rankPair.Value);

        //finding ou if there is ranks with same card counts
        bool duplicatesExist = false;

        int valueCounter = 0;
        foreach (var iValue in cardsCounterSortedVessel.Values)
        {
            foreach (byte jValue in cardsCounterSortedVessel.Values)
            {
                if (iValue == jValue)
                    valueCounter++;
            }
            if (valueCounter > 1)
            {
                duplicatesExist = true;
                break;
            }
            else
                valueCounter = 0;
        }
        if (duplicatesExist)
        {
            if (cardsCounterSortedVessel.Count == 2)
            {
                //converting duplicates to brute value
                cardsCounterSortedVessel.ByteToBruteValue();
                // sorting
                Dictionary<byte, byte> rankSortedVessel = cardsCounterSortedVessel.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                // converting the sorted dic to byte
                rankSortedVessel.BruteValueToByte();
            }
            else
            {
                //extracting duplicates
                Dictionary<byte, byte> SelectedDuplicates = cardsCounterSortedVessel.Where(x => x.Value == CardManager.MaxRankCounter).ToDictionary(x => x.Key, x => x.Value);
                //at this step only locked ranks duplicates should exist
                if (SelectedDuplicates.Count < 2)
                {
#if Log
                    LogManager.LogError("Sorting Bet Failed  ! SelectedDuplicates Count is Invalid!");
#endif
                    return;
                }
                //removing the selected ranks
                foreach (var item in SelectedDuplicates)
                {
                    cardsCounterSortedVessel.Remove(item.Key);
                }
                // casualy converting then sorting procedure
                SelectedDuplicates.ByteToBruteValue();
                Dictionary<byte, byte> sortedSelectedDuplicates = SelectedDuplicates.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                sortedSelectedDuplicates.BruteValueToByte();
                //adding left over elements PS there should only one

                if (cardsCounterSortedVessel.Count == 1)
                {
                    var leftoverelement = cardsCounterSortedVessel.First();
                    sortedSelectedDuplicates.Add(leftoverelement.Key, leftoverelement.Value);
                }
                else
                {
#if Log
                    LogManager.LogError($"Sorting Bet Failed  ! there should one element left incardsCounterSortedVessel but count is =:{cardsCounterSortedVessel.Count} !");
#endif
                    return;
                }
            }
        }
    }

    public static void BetDiffuserAlpha(byte[] Bet, Dictionary<byte, byte> diffusedBet, int index)
    {
        if (index >= Bet.Length) return;
        if (!diffusedBet.IsRankDiffused(Bet[index]))
        {
            diffusedBet.Add(Bet[index], (byte)RankCounterAlpha(Bet, Bet[index], 0, 0));
            BetDiffuserAlpha(Bet, diffusedBet, ++index);
        }
        BetDiffuserAlpha(Bet, diffusedBet, ++index);
    }

    public static byte[] SortBet(this byte[] bet)
    {
        byte[] sortedBet = new byte[bet.Length];
        Array.Copy(bet, sortedBet, bet.Length);
        //converting bet array to brute value
        sortedBet.ByteToBruteValue();
        //sorting decendent
        sortedBet.OrderBy(b => b);
        //converting array back to ranks
        sortedBet.BruteValueToByteAlpha(0);
        return sortedBet;
    }

    public static void ByteToBruteValue(this byte[] bet)
    {
        int bruteValue;
        byte rank;
        for (int index = 0; index < bet.Length; index++)
        {
            rank = bet[index];
            if (CardManager.SortedRanks.TryGetRankBruteValueAlpha(rank, 0, out bruteValue))
            {
                bet[index] = (byte)(bruteValue + 1);
            }
            else
            {
#if Log
                LogManager.LogError($"Failed converting Rank={rank} to brute Value!");
#endif
                return;
            }
        }
    }

    public static void ByteToBruteValue(this Dictionary<byte, byte> bet)
    {
        Dictionary<byte, byte> bruteValueBet = new Dictionary<byte, byte>();
        int bruteValue;
        byte rank;
        foreach (var item in bet)
        {
            rank = item.Key;
            if (CardManager.SortedRanks.TryGetRankBruteValueAlpha(rank, 0, out bruteValue))
            {
                bruteValueBet.Add((byte)(bruteValue + 1), item.Value);
            }
            else
            {
#if Log
                LogManager.LogError($"Failed converting Rank={rank} to brute Value!");
#endif
                return;
            }
        }

        bet.Clear();
        foreach (var item in bruteValueBet)
        {
            bet.Add(item.Key, item.Value);
        }
    }

    public static void BruteValueToByte(this byte[] bet)
    {
        byte rank;
        for (int index = 0; index < bet.Length; index++)
        {
            rank = CardManager.SortedRanks[bet[index] - 1];
            bet[index] = rank;
        }
    }

    public static void BruteValueToByte(this Dictionary<byte, byte> bruteValueBet)
    {
        Dictionary<byte, byte> bet = new Dictionary<byte, byte>();
        byte rank;
        foreach (var item in bruteValueBet)
        {
            rank = CardManager.SortedRanks[item.Key - 1];
            bet.Add(rank, item.Value);
        }
        bruteValueBet.Clear();
        foreach (var item in bet)
        {
            bruteValueBet.Add(item.Key, item.Value);
        }
    }

    public static void BruteValueToByteAlpha(this byte[] bet, int index)
    {
        if (index >= bet.Length) return;
        byte rank = CardManager.SortedRanks[bet[index]];
        bet[index] = rank;
        BruteValueToByteAlpha(bet, ++index);
    }

    public static byte[] ToByteArray(this List<byte> list)
    {
        byte[] bytes = new byte[list.Count];
        int index = 0;
        foreach (byte b in list)
        {
            bytes[index++] = b;
        }
        return bytes;
    }

    /// <summary>
    /// both arrays should be sorted before hand
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool AreEqual(this byte[] a, byte[] b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// both arrays should be sorted before hand
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool AreEqualAlpha(this byte[] a, byte[] b, int index)
    {
        if (index >= a.Length) return true;
        if (a[index] != b[index]) return false;
        return AreEqualAlpha(a, b, ++index);
    }

    #endregion byte Array

    #region BetGeneration

    public static byte[] GenerateMaxBet(int dealtCardsNumber)
    {
        byte[] MaxBet;
        List<byte> maxBetBytes = new List<byte>();
        int MaxValueRankIndex = CardManager.SortedRanks.Length - 1;
        //getting the max value rank
        byte MaxValueRank = CardManager.SortedRanks[MaxValueRankIndex];
        // if the dealt cards are less or equal to Rank counter (dealt cards should never be one )
        if (dealtCardsNumber <= CardManager.MaxRankCounter)
        {
            //filling max bet with max valued rank
            for (int index = 0; index < dealtCardsNumber; index++)
            {
                maxBetBytes.Add(MaxValueRank);
            }
            //converting list
            MaxBet = maxBetBytes.ToByteArray();
            return MaxBet;
        }

        int LockedRanksCounter = dealtCardsNumber / CardManager.MaxRankCounter;
        // making sure all possible ranks are Locked
        do
        {
            //filling max bet with a Locked max Value Rank
            for (int index = 0; index < CardManager.MaxRankCounter; index++)
            {
                maxBetBytes.Add(MaxValueRank);
            }
            --LockedRanksCounter;
            --MaxValueRankIndex;
            MaxValueRank = CardManager.SortedRanks[MaxValueRankIndex];
        }
        while (LockedRanksCounter > 0);
        // if lef over spots are more then one then we filling em
        int leftOverSpots = dealtCardsNumber % CardManager.MaxRankCounter;
        if (leftOverSpots > 1)
        {
            for (int index = 0; index < leftOverSpots; index++)
            {
                maxBetBytes.Add(MaxValueRank);
            }
        }
        //converting list
        MaxBet = maxBetBytes.ToByteArray();
        return MaxBet;
    }

    public static bool TryRoundUpBet(byte[] bet, out byte[] roundedUpBet, int dealtCardsCount)
    {
        int BetTotalCardsCount = bet.Length;
        roundedUpBet = new byte[BetTotalCardsCount];
        // cant round up an invalid Bet
        if (dealtCardsCount < BetTotalCardsCount)
        {
#if Log
            LogManager.LogError("Failed to Round Up Bet" + string.Join(",", bet) + " Bet Total Cards are more then the dealt Cards Counter !");
#endif
            return false;
        }

        bool betIsRoundedUp = false;

        //sorting bet A>=B form to manage easeally
        byte[] sotedBet = bet.SortBet();

        byte[] maxBet = GenerateMaxBet(BetTotalCardsCount);
        // cant round up bet if it is already maxed
        if (maxBet.AreEqualAlpha(sotedBet, 0))
        {
            if (dealtCardsCount == BetTotalCardsCount)
            {
#if Log
                LogManager.Log("Failed to Round Up Bet" + string.Join(",", bet) + " is already Maxed Out !", Color.yellow, LogManager.Validators);
#endif
                return false;
            }
            else
            {
                //proceed to second type of rounding up a bet
            }
        }
        bool changesAreMade = false;
        Dictionary<byte, byte> diffusedBet = new Dictionary<byte, byte>();
        List<byte> RanksToRoundUp = new List<byte>();
        // diffusing bet to ease management
        BetDiffuserAlpha(bet, diffusedBet, 0);
        // trying to update non Locked ranks first
        foreach (byte rank in diffusedBet.Keys.Reverse())
        {
            //ignoring if it is a Locked Rank
            if (diffusedBet[rank] == CardManager.MaxRankCounter) continue;
        }
        return betIsRoundedUp;
    }

    #endregion BetGeneration
}