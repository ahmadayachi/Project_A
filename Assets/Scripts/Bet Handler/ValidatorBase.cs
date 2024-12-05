using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ValidatorBase
{
#if Log

    #region Log stuff

    protected const string LevelOne = "Level One ";
    protected const string LevelTwo = "Level Two ";
    protected const string LevelThree = "Level Three ";
    protected const string ArgsNotValid = "ARGS are not valid!";
    protected const string BetPassValidation = "Bet Pass Validation";
    protected const string BetFailedValidation = "Bet Failed Validation";
    protected const string CurrentBetIsSmaller = "Current Bet  cant be smaller the previous Bet!";
    protected const string Bet = "Bet = : ";
    #endregion Log stuff

#endif
    public const int LockedRankBruteValueBuffer = 69;
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
        bool rankValueExists = CardManager.SortedRanks.TryGetRankBruteValueAlpha(Rank, 0, out rankValue);
        bool rankToCompareValueExists = CardManager.SortedRanks.TryGetRankBruteValueAlpha(RankToCompare, 0, out rankToCompareValue);

        if (!rankValueExists || !rankToCompareValueExists)
        {
#if Log
            LogManager.LogError($"Failed to fetch Rank value! Rank = {Rank}, Rank to compare = {RankToCompare}");
#endif
            return false;
        }
        return rankValue < rankToCompareValue;
    }

    //protected int RankCounter(byte[] bet, byte rank)
    //{
    //    int counter = 0;
    //    foreach (var item in bet)
    //    {
    //        if (item == rank)
    //            counter++;
    //    }
    //    return counter;
    //}
    //protected int RankCounterAlpha(byte[] Bet, byte rank, int Index, int Counter)
    //{
    //    if (Index >= Bet.Length) return Counter;
    //    if (Bet[Index] == rank) return RankCounterAlpha(Bet, rank, ++Index, ++Counter);
    //    return RankCounterAlpha(Bet, rank, ++Index, Counter);
    //}

    protected bool DiffusedBetRanksCounterNotValid(Dictionary<byte, byte> bet)
    {
        bool notvalid = false;
        foreach (var item in bet)
        {
            if (item.Value <= 1 || item.Value > CardManager.MaxRankCounter)
                return true;
        }
        return notvalid;
    }

    //protected void BetDiffuser(byte[] Bet, Dictionary<byte, byte> diffusedBet)
    //{
    //    foreach (byte rank in Bet)
    //    {
    //        if (diffusedBet.IsRankDiffused(rank))
    //            continue;
    //        else
    //            diffusedBet.Add(rank, (byte)RankCounterAlpha(Bet, rank, 0, 0));
    //    }
    //}

    //protected void BetDiffuserAlpha(byte[] Bet, Dictionary<byte, byte> diffusedBet, int index)
    //{
    //    if (index >= Bet.Length) return;
    //    if (!diffusedBet.IsRankDiffused(Bet[index]))
    //    {
    //        diffusedBet.Add(Bet[index], (byte)RankCounterAlpha(Bet, Bet[index], 0, 0));
    //        BetDiffuserAlpha(Bet, diffusedBet, ++index);
    //    }
    //    BetDiffuserAlpha(Bet, diffusedBet, ++index);
    //}

    protected bool IsDiffusedBetNotValid(Dictionary<byte, byte> diffusedBet)
    {
        // counter the ranks cards counter that is less the max ranks counter
        int lessthenFullSetCounter = 0;
        // counter for a ranks cards that equall to the max ranks counter
        int fullSetCounter = 0;
        foreach (byte rankCounter in diffusedBet.Values)
        {
            if (rankCounter == CardManager.MaxRankCounter)
                fullSetCounter++;
            if (rankCounter < CardManager.MaxRankCounter)
                lessthenFullSetCounter++;
        }

        if (fullSetCounter >= 1)
        {
            if (lessthenFullSetCounter > 1)
                return true;
        }
        else
        {
            if (lessthenFullSetCounter > 2)
                return true;
        }

        return false;
    }

    protected int DiffusedBetToBruteValue(Dictionary<byte, byte> diffusedDeck)
    {
        int bruteValue = 0;
        int rankValue = 0;
        bool rankValueExists;
        foreach (var rankPair in diffusedDeck)
        {
            rankValueExists = CardManager.SortedRanks.TryGetRankBruteValueAlpha(rankPair.Key, 0, out rankValue);
            if (rankValueExists)
            {
                bruteValue += (rankValue + 1) * (rankPair.Value==CardManager.MaxRankCounter?(rankPair.Value*LockedRankBruteValueBuffer): rankPair.Value);
            }
            else
            {
#if Log
                LogManager.LogError($"Failed to fetch Rank value! Rank = {rankPair.Key}");
#endif
                break;
            }
        }
        if (bruteValue == 0)
        {
#if Log
            LogManager.LogError($"Diffused Deck Brute Value Cant Be 0!");
#endif
        }
        return bruteValue;
    }

    protected bool IsSmallerBetNotValid(Dictionary<byte, byte> bet, Dictionary<byte, byte> previousBet)
    {
        int cardsCountCounter = 0;
        foreach (var previousbetPair in previousBet)
        {
            foreach (var betPair in bet)
            {
                if (previousbetPair.Value < betPair.Value)
                    cardsCountCounter++;
                else
                    return true;
            }
        }

        return cardsCountCounter < 1;
    }
}