using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ValidatorBase
{
    protected bool ValidBetArgs(ValidatorArguments arguments)
    {
        return ((!arguments.CurrentBet.IsNullOrEmpty()) && (arguments.PreviousBet != null));
    }

    /// <summary>
    /// true if Rank to compare is higher in value
    /// </summary>
    /// <param name="Rank"></param>
    /// <param name="RankToCompare"></param>
    /// <returns></returns>
    protected bool IsRankHigherInValue(byte Rank, byte RankToCompare)
    {
        int rankValue = 0;
        int rankToCompareValue = 0;
        bool rankValueExists = CardManager.SortedRanks.GetRankValue(Rank, 0, out rankValue);
        bool rankToCompareValueExists = CardManager.SortedRanks.GetRankValue(RankToCompare, 0, out rankToCompareValue);

        if (!rankValueExists || !rankToCompareValueExists)
        {
#if Log
            LogManager.LogError($"Failed to fetch Rank value! Rank = {Rank}, Rank to compare = {RankToCompare}");
#endif
            return false;
        }
        return rankValue < rankToCompareValue;
    }

    //protected List<byte> ConvertToAllUsedRanks(byte[] Bet)
    //{
    //    var allUsedRanks = new List<byte>();
    //    foreach (byte bet in Bet)
    //    {
    //        if (!allUsedRanks.Contains(bet))
    //            allUsedRanks.Add(bet);
    //    }
    //    return allUsedRanks;
    //}
    protected void SetUpAllUsedRanksAlpha(byte[] bet, List<byte> allUsedRanks, int index)
    {
        if (index >= bet.Length)
            return;
        if (!allUsedRanks.Contains(bet[index]))
        {
            allUsedRanks.Add(bet[index]);
            SetUpAllUsedRanksAlpha(bet, allUsedRanks, ++index);
        }
        SetUpAllUsedRanksAlpha(bet, allUsedRanks, ++index);
    }

    //protected int RankCounterBeta(byte[] bet, byte rank)
    //{
    //    int counter = 0;
    //    foreach (var item in bet)
    //    {
    //        if (item == rank)
    //            counter++;
    //    }
    //    return counter;
    //}
    protected int RankCounterAlpha(byte[] Bet, byte rank, int Index, int Counter)
    {
        if (Index >= Bet.Length)
            return Counter;
        if (Bet[Index] == rank)
            return RankCounterAlpha(Bet, rank, ++Index, ++Counter);

        return RankCounterAlpha(Bet, rank, ++Index, Counter);
    }

    protected bool AllUsedRanksValid(List<byte> allUsedRank, byte[] bet)
    {
        int rankCounter;
        foreach (byte rank in allUsedRank)
        {
            rankCounter = RankCounterAlpha(bet, rank, 0, 0);
            if (rankCounter == 0)
            {
#if Log
                LogManager.LogError("Invalid Rank Counter! Rank counter cant be 0!");
#endif
                return false;
            }
            if (rankCounter > CardManager.RankCounter)
                return false;
        }
        return true;
    }
}