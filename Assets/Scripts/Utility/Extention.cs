using Codice.Client.BaseCommands.BranchExplorer;
using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

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
    public static byte[] ToByteArray(this NetworkArray<byte> array)
    {
        int arrayCount = array.ValidCardsCount();
        if (arrayCount == 0)
        {
            //returning an rmpty array
            return new byte[] { };
        }
        byte[] convertedArray = new byte[arrayCount];
        int jindex = 0;
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index] != 0))
                convertedArray[jindex++] = array[index];
        }
        return convertedArray;
    }
    public static List<byte> ToByteList(this NetworkArray<byte> array)
    {
        int arrayCount = array.ValidCardsCount();
        List<byte> byteList = new List<byte> ();
        if (arrayCount == 0)
        {
            //returning an rmpty array
            return byteList;
        }
        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index] != 0))
                byteList.Add(array[index]);
        }
        return byteList;
    }
    public static void ToByteList(this NetworkArray<byte> array, List<byte> byteList)
    {
        byteList.Clear();
        int arrayCount = array.ValidCardsCount();

        if (arrayCount == 0) return;

        for (int index = 0; index < array.Length; index++)
        {
            if ((array[index] != 0))
                byteList.Add(array[index]);
        }
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

    public static bool TryGetRankBruteValue(this byte[] array, byte rank, out int Value)
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

    public static bool TryRoundUpRank(this byte[] sortedRanks, byte rank, out byte roundedUpRank)
    {
        roundedUpRank = 0;
        int rankValue = 0;
        bool isRounded = false;
        if (sortedRanks.TryGetRankBruteValueAlpha(rank, 0, out rankValue))
        {
            //cant round up a max value rank
            if (rankValue == sortedRanks.Length - 1)
            {
#if Log
                LogManager.Log($"Failed Rounding Up a Rank=>{rank} because it is already max value !", Color.yellow, LogManager.ValueInformationLog);
#endif
                return isRounded;
            }
            else
            {
                //geting the rank right after it
                roundedUpRank = CardManager.SortedRanks[rankValue + 1];
                isRounded = true;
            }
        }
        else
        {
#if Log
            LogManager.LogError($"Failed Rounding Up and Invalid Rank=>{rank}");
#endif
            return isRounded;
        }
        return isRounded;
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

    /// <summary>
    /// Bet Should be Valid before Sorted !
    /// </summary>
    /// <param name="bet"></param>
    public static void SortBet(this Dictionary<byte, byte> bet)
    {
        // returning if there is only one set of ranks
        if (bet.Count == 1) return;

        Dictionary<byte, byte> rankSortedVessel;

        //first sorting with Card Count a >= b format
        Dictionary<byte, byte> cardsCounterSortedVessel = bet.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        bool duplicatesExist = false;
        int valueCounter = 0;

        //finding ou if there is ranks with same card counts
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
                rankSortedVessel = cardsCounterSortedVessel.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
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
                    cardsCounterSortedVessel.Remove(item.Key);
                // casualy converting then sorting procedure
                SelectedDuplicates.ByteToBruteValue();
                rankSortedVessel = SelectedDuplicates.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                rankSortedVessel.BruteValueToByte();
                //adding left over elements PS there should only one
                if (cardsCounterSortedVessel.Count == 1)
                {
                    var leftoverelement = cardsCounterSortedVessel.First();
                    rankSortedVessel.Add(leftoverelement.Key, leftoverelement.Value);
                }
                else
                {
#if Log
                    LogManager.LogError($"Sorting Bet Failed  ! there should one element left but count is =:{cardsCounterSortedVessel.Count} !");
#endif
                    return;
                }
            }
        }
        else
            rankSortedVessel = cardsCounterSortedVessel;

        //copying shit back
        bet.Clear();
        foreach (var rankPair in rankSortedVessel)
            bet.Add(rankPair.Key, rankPair.Value);
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

    public static byte[] ToByteArray(this Dictionary<byte, byte> bet)
    {
        if (bet.Count == 0) return null;

        byte[] betArray = new byte[bet.BetTotalCardsCount()];
        int betIndex = 0;
        foreach (var item in bet)
        {
            for (int index = 0; index < item.Value; index++)
                betArray[betIndex++] = item.Key;
        }
        return betArray;
    }
    public static byte[] ToByteArray (this List<DiffusedRankInfo> bet)
    {
        if (bet.Count == 0) return null;

        byte[] betArray = new byte[bet.BetTotalCardsCount()];
        int betIndex = 0;
        foreach (var item in bet)
        {
            for (int index = 0; index < item.CardsCount; index++)
                betArray[betIndex++] = item.Rank;
        }
        return betArray;
    }
    
    public static int BetTotalCardsCount(this Dictionary<byte, byte> bet)
    {
        int betTotalCardsCount = 0;
        foreach (var item in bet)
        {
            betTotalCardsCount += item.Value;
        }
        return betTotalCardsCount;
    }
    public static int BetTotalCardsCount(this List<DiffusedRankInfo> bet)
    {
        int betTotalCardsCount = 0;
        foreach (var item in bet)
        {
            betTotalCardsCount += item.CardsCount;
        }
        return betTotalCardsCount;
    }
    /// <summary>
    /// both arrays should be sorted before hand
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool AreEqual(this byte[] a, byte[] b)
    {
        if (a.IsNullOrEmpty() || b.IsNullOrEmpty()) return false;
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    /// <summary>
    /// true if both arrays simalir card by card PS they should be sorted before hand
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

   

    #region List of Diffused rank Struct shit
    public static bool IsBetRankNonSuccessive(this List<DiffusedRankInfo> betList,out DiffusedRankInfo BetRank )
    {
        BetRank = new DiffusedRankInfo();
        for (int index = 0; index < betList.Count - 1; index++)
        {
            if (betList[index].RankBruteValue - betList[index + 1].RankBruteValue > 1)
            {
               BetRank= betList[index+1];
                return true;
            }
        }
        return false;
    }
    public static bool IsRankDiffused(this List<DiffusedRankInfo> diffusedBet, byte rank)
    {
        if (diffusedBet.Count == 0) return false;
        foreach (var rankInfo in diffusedBet)
        {
            if (rankInfo.Rank == rank)
                return true;
        }
        return false;
    }

    public static void BetDiffuser(byte[] bet, List<DiffusedRankInfo> diffusedBet)
    {
        diffusedBet.Clear();
        int rankBruteValue = 0;
        foreach (byte rank in bet)
        {
            if (diffusedBet.IsRankDiffused(rank))
                continue;
            else
            {
                if (CardManager.SortedRanks.TryGetRankBruteValue(rank, out rankBruteValue))
                {
                    DiffusedRankInfo diffusedRankInfo = new DiffusedRankInfo();
                    diffusedRankInfo.Rank = rank;
                    diffusedRankInfo.RankBruteValue = (rankBruteValue + 1);
                    diffusedRankInfo.CardsCount = (byte)RankCounter(bet, rank);
                    diffusedBet.Add(diffusedRankInfo);
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
    }
    /// <summary>
    /// Bet Should be Valid before Sorted !
    /// </summary>
    /// <param name="bet"></param>
    public static void SortDiffusedBet(this List<DiffusedRankInfo> diffusedBet)
    {
        // returning if there is only one set of ranks
        if (diffusedBet.Count == 1) return;

        List<DiffusedRankInfo> rankSortedVessel;

        //first sorting with Card Count a >= b format
        var cardsCounterSortedVessel = diffusedBet.OrderByDescending(x => x.CardsCount).ToList();

        bool duplicatesExist = false;
        int valueCounter = 0;
        //finding ou if there is ranks with same card counts
        foreach (var iValue in diffusedBet)
        {
            foreach (var jValue in diffusedBet)
            {
                if (iValue.CardsCount == jValue.CardsCount)
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
                // sorting
                rankSortedVessel = cardsCounterSortedVessel.OrderByDescending(x => x.RankBruteValue).ToList();
            }
            else
            {
                //extracting duplicates
                var SelectedDuplicates = cardsCounterSortedVessel.Where(x => x.CardsCount == CardManager.MaxRankCounter).ToList();
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
                    cardsCounterSortedVessel.Remove(item);
                // casual sorting procedure
                rankSortedVessel = SelectedDuplicates.OrderByDescending(x => x.RankBruteValue).ToList();


                int NonLockedRanksCounter = 0;
                //first need to check if there are any Non Locked Ranks 
                if (diffusedBet.IncludeNonLockedRank(out NonLockedRanksCounter))
                {

                    //adding left over element PS there should only one
                    if (cardsCounterSortedVessel.Count == 1)
                    {
                        var leftoverelement = cardsCounterSortedVessel.First();
                        rankSortedVessel.Add(leftoverelement);
                    }
                    else
                    {
#if Log
                        LogManager.LogError($"Sorting Bet Failed  ! there should one element left but count is =:{cardsCounterSortedVessel.Count} !");
#endif
                        return;
                    }
                }
            }
        }
        else
            rankSortedVessel = cardsCounterSortedVessel;

        //copying shit back
        diffusedBet.Clear();
        diffusedBet.AddRange(rankSortedVessel);
    }
    public static bool IncludeNonLockedRank(this List<DiffusedRankInfo> diffusedbet, out int counter)
    {
        counter = 0;
        bool NonLockedExist = false;
        foreach (var item in diffusedbet)
        {
            if (item.CardsCount < CardManager.MaxRankCounter)
            {
                counter++;
                NonLockedExist = true;
            }
        }
        return NonLockedExist;
    }
    #endregion
}