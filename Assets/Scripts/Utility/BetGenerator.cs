using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BetGenerator
{
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

        //Launching a Bet when there is no Previous Bet

        //for Ill check for valid cards Count assuimg ill be getting the bet array directly from a networked array and it cant be null
        if (bet.ValidCardsCount() == 0)
        {
            // the dealt Cards Can Not be < then 2
            if (dealtCardsCount < 2)
            {
#if Log
                LogManager.LogError($"Failed to Launch Bet!, Dealt Cards Counter is {dealtCardsCount} < 2 ");
#endif
                return false;
            }
            // a 40 percent chance to launche with two cards count
            int rankCardCounter = Random.value <= 0.4f ? 2 : 1;

            //proceeding to randomly picking a rank
            byte rank = CardManager.SortedRanks[Random.Range(0, CardManager.SortedRanks.Length)];
            //I think I have to resize the bet array
            roundedUpBet = new byte[rankCardCounter];
            //filling bet
            for (int index = 0; index < rankCardCounter; index++)
            {
                roundedUpBet[index] = rank;
            }
            return true;
        }

        List<DiffusedRankInfo> diffusedBet = new List<DiffusedRankInfo>();
        // diffusing bet to ease management
        Extention.BetDiffuser(bet, diffusedBet);
        //sorting bet A>=B form to manage easeally
        diffusedBet.SortDiffusedBet();
        //converting back sorted deck to compare it
        byte[] sotedBet = diffusedBet.ToByteArray();
        byte[] maxBet = GenerateMaxBet(BetTotalCardsCount);
        // cant round up bet if it is already maxed
        if (maxBet.AreEqualAlpha(sotedBet, 0))
        {
            if (dealtCardsCount == BetTotalCardsCount /*|| (dealtCardsCount - 1) == BetTotalCardsCount*/)
            {
#if Log
                LogManager.Log("Failed to Round Up Bet" + string.Join(",", bet) + " is already Maxed Out !", Color.yellow, LogManager.Validators);
#endif
                return false;
            }
            else
            {
                //<================= Second type of Rounding Up =====================>
                //first need to check if there are any Non Locked Ranks 


            }
        }
        else
        {
            //Rounding Up a bet with Size 1 Rank N>=1/8 Total Cards
            if (diffusedBet.Count == 1)
            {
                DiffusedRankInfo betDiffusedRank = diffusedBet[0];
                byte roudededRank = 0;
                if (CardManager.SortedRanks.TryRoundUpRank(betDiffusedRank.Rank, out roudededRank))
                {
                    //replacing cards with a rounded up rank cards
                    for (int index = 0; index < betDiffusedRank.CardsCount; index++)
                    {
                        roundedUpBet[index] = roudededRank;
                    }
                    return true;
                }
                else
                {
#if Log
                    LogManager.Log($"Failed to Round Up Rank{betDiffusedRank.Rank}", Color.yellow, LogManager.Validators);
#endif
                    return false;
                }
            }
            else
            //Rounding Up a bet with Size 2 Rank N>1/8 Total Cards
            if (diffusedBet.Count == 2)
            {
                DiffusedRankInfo firstRank = diffusedBet[0];
                DiffusedRankInfo secondRank = diffusedBet[1];
                //checking if the two ranks that are played are two highest ranks
                if (firstRank.RankBruteValue == (CardManager.SortedRanks.Length - 1) && secondRank.RankBruteValue == (CardManager.SortedRanks.Length - 2))
                {
                    //<================= Second type of Rounding Up =====================>
                }
                else
                {
                    byte roudededRank = 0;
                    int rankToRoundUpIndex = 0;
                    //checking if the two rank are not successive to round up the lowest rank in value
                    if ((firstRank.RankBruteValue - secondRank.RankBruteValue) > 1)
                    {
                        rankToRoundUpIndex = 1;
                    }
                    DiffusedRankInfo RankToRoundUp = diffusedBet[rankToRoundUpIndex];
                    if (CardManager.SortedRanks.TryRoundUpRank(RankToRoundUp.Rank, out roudededRank))
                    {
                        //replacing Pair in list leaving the sorting untouched

                        int rankBruteValue;
                        if (CardManager.SortedRanks.TryGetRankValue(roudededRank, out rankBruteValue))
                        {
                            //setting rounded rank info
                            DiffusedRankInfo newLastBetInfo = new DiffusedRankInfo();
                            newLastBetInfo.Rank = roudededRank;
                            newLastBetInfo.RankBruteValue = rankBruteValue;
                            newLastBetInfo.CardsCount = RankToRoundUp.CardsCount;
                            diffusedBet[rankToRoundUpIndex] = newLastBetInfo;

                            roundedUpBet = diffusedBet.ToByteArray();
                            return true;
                        }
                        else
                        {
#if Log
                            LogManager.LogError($"Failed converting Rank={roudededRank} to brute Value!");
#endif
                            return false;
                        }
                    }
                    else
                    {
#if Log
                        LogManager.Log($"Failed to Round Up Rank{RankToRoundUp.Rank}", Color.yellow, LogManager.Validators);
#endif
                        return false;
                    }
                }
            }
            else
            // Rounding Up a bet with Size 	N>2 Rank N>1/8 Total Cards
            if (diffusedBet.Count > 2)
            {
                //checking if the last rank in this bet is not Locked
                DiffusedRankInfo lastBetInfo = diffusedBet[diffusedBet.Count - 1];
                if (lastBetInfo.CardsCount != CardManager.MaxRankCounter)
                {
                    byte roundededRank = 0;
                    if (CardManager.SortedRanks.TryRoundUpRank(lastBetInfo.Rank, out roundededRank))
                    {
                        if (!diffusedBet.IsRankDiffused(roundededRank))
                        {
                            int rankBruteValue;
                            if (CardManager.SortedRanks.TryGetRankValue(roundededRank, out rankBruteValue))
                            {
                                //setting rounded rank info
                                DiffusedRankInfo newLastBetInfo = new DiffusedRankInfo();
                                newLastBetInfo.Rank = roundededRank;
                                newLastBetInfo.RankBruteValue = rankBruteValue;
                                newLastBetInfo.CardsCount = lastBetInfo.CardsCount;
                                diffusedBet[diffusedBet.Count - 1] = newLastBetInfo;

                                roundedUpBet = diffusedBet.ToByteArray();
                                return true;
                            }
                            else
                            {
#if Log
                                LogManager.LogError($"Failed converting Rank={roundededRank} to brute Value!");
#endif
                                return false;
                            }
                        }
                    }
                }
                // cheking if bet contains non successive ranks
                DiffusedRankInfo NonSuccessiveRank;
                if (diffusedBet.IsBetRankNonSuccessive(out NonSuccessiveRank))
                {
                    byte roundededRank = 0;
                    if (CardManager.SortedRanks.TryRoundUpRank(NonSuccessiveRank.Rank, out roundededRank))
                    {
                        //if rounded rank doesnt exist in bet then rounding up complete
                        if (!diffusedBet.IsRankDiffused(roundededRank))
                        {
                            int rankBruteValue;
                            if (CardManager.SortedRanks.TryGetRankValue(roundededRank, out rankBruteValue))
                            {
                                //setting rounded rank info
                                DiffusedRankInfo newRoundedBetInfo = new DiffusedRankInfo();
                                newRoundedBetInfo.Rank = roundededRank;
                                newRoundedBetInfo.RankBruteValue = rankBruteValue;
                                newRoundedBetInfo.CardsCount = NonSuccessiveRank.CardsCount;

                                for (int index = 0; index < diffusedBet.Count; index++)
                                {
                                    if (diffusedBet[index].Rank == NonSuccessiveRank.Rank)
                                    {
                                        diffusedBet[index] = newRoundedBetInfo;
                                    }
                                }

                                roundedUpBet = diffusedBet.ToByteArray();
                                return true;
                            }
                            else
                            {
#if Log
                                LogManager.LogError($"Failed converting Rank={roundededRank} to brute Value!");
#endif
                                return false;
                            }
                        }
                        else
                        //if rounded up rank exists then switch them
                        {
                            //if rounded up rank exists then it mus a non Locked rank (list is sorted so it should last element here )
                            DiffusedRankInfo existingRoundedUpRank = diffusedBet[diffusedBet.Count - 1];
                            if (existingRoundedUpRank.Rank != roundededRank)
                            {
#if Log
                                LogManager.LogError($"Failed Rounding Up Bet! Invalid Bet =>{string.Join(",", diffusedBet)} Rank to roundup{NonSuccessiveRank.Rank} rounded rank {roundededRank}");
#endif
                                return false;
                            }

                            for (int index = 0; index < diffusedBet.Count; index++)
                            {
                                if (diffusedBet[index].Rank == NonSuccessiveRank.Rank)
                                {
                                    var newRounduprankInfo = new DiffusedRankInfo();
                                    newRounduprankInfo.Rank = existingRoundedUpRank.Rank;
                                    newRounduprankInfo.RankBruteValue = existingRoundedUpRank.RankBruteValue;
                                    newRounduprankInfo.CardsCount = NonSuccessiveRank.CardsCount;

                                    var newLastRankInfo = new DiffusedRankInfo();
                                    newLastRankInfo.Rank = NonSuccessiveRank.Rank;
                                    newLastRankInfo.RankBruteValue = NonSuccessiveRank.RankBruteValue;
                                    newLastRankInfo.CardsCount = existingRoundedUpRank.CardsCount;
                                    //switching places
                                    diffusedBet[index] = newRounduprankInfo;
                                    diffusedBet[diffusedBet.Count - 1] = newLastRankInfo;
                                    break;
                                }
                            }

                            roundedUpBet = diffusedBet.ToByteArray();
                            return true;
                        }
                    }
                }
                else
                //if bet successive  I simply round up the first Rank
                {
                    DiffusedRankInfo betToRoundUp = diffusedBet[0];
                    byte roundededRank = 0;

                    if (CardManager.SortedRanks.TryRoundUpRank(betToRoundUp.Rank, out roundededRank))
                    {
                        int rankBruteValue;
                        if (CardManager.SortedRanks.TryGetRankValue(roundededRank, out rankBruteValue))
                        {
                            //setting rounded rank info
                            var newRoundedBetInfo = new DiffusedRankInfo();
                            newRoundedBetInfo.Rank = roundededRank;
                            newRoundedBetInfo.RankBruteValue = betToRoundUp.RankBruteValue;
                            newRoundedBetInfo.CardsCount = betToRoundUp.CardsCount;

                            diffusedBet[0] = newRoundedBetInfo;

                            roundedUpBet = diffusedBet.ToByteArray();
                            return true;
                        }
                        else
                        {
#if Log
                            LogManager.LogError($"Failed converting Rank={roundededRank} to brute Value!");
#endif
                            return false;
                        }
                    }
                    else
                    {
#if Log
                        LogManager.LogError($"Failed to Round Up Rank{betToRoundUp.Rank} bet is Successive and is not a maxed Bet!");
#endif
                        return false;
                    }
                }
            }
        }
        return false;
    }
}