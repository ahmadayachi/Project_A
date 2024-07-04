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
        //making sure that cards count is a valid cards count not just empty cases in array
        int BetTotalCardsCount = bet.ValidCardsCount();
        roundedUpBet = new byte[BetTotalCardsCount];
        // cant round up an invalid Bet
        if (dealtCardsCount < BetTotalCardsCount)
        {
#if Log
            LogManager.LogError("Failed to Round Up Bet" + string.Join(",", bet) + " Bet Total Cards are more then the dealt Cards Counter !");
#endif
            return false;
        }

        //<======Launching a Bet when there is no Previous Bet=======>

        //check for valid cards Count assuimg ill be getting the bet array directly from a networked array and it cant be null
        if (bet.ValidCardsCount() == 0)
        {
            return LaunchBet(ref roundedUpBet, dealtCardsCount);
        }
        //<=========================================================>

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
            if (dealtCardsCount == BetTotalCardsCount)
            {
#if Log
                LogManager.Log("Failed to Round Up Bet" + string.Join(",", bet) + " is already Maxed Out !", Color.yellow, LogManager.Validators);
#endif
                return false;
            }
            else
            {
                //<================= Second type of Rounding Up =====================>
                return RoundUpMaxedBet(bet, ref roundedUpBet, dealtCardsCount, BetTotalCardsCount, diffusedBet);
                //<=========================================================>
            }
        }
        //<================= First type of Rounding Up =====================>
        else
        {
            switch (diffusedBet.Count)
            {
                //Rounding Up a bet with Size 1 Rank N>=1/8 Total Cards
                case 1: return OneRankBetRoundUp(roundedUpBet, diffusedBet);
                //Rounding Up a bet with Size 2 Rank N>1/8 Total Cards
                case 2: return TwoRankBetRoundUp(bet, ref roundedUpBet, diffusedBet);
                // Rounding Up a bet with Size 	N>2 Rank N>1/8 Total Cards
                case > 2: return MultiRankRoundUp(ref roundedUpBet, diffusedBet);
            }
        }
        //<=========================================================>
        return false;
    }

    private static bool MultiRankRoundUp(ref byte[] roundedUpBet, List<DiffusedRankInfo> diffusedBet)
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
                    if (CardManager.SortedRanks.TryGetRankBruteValue(roundededRank, out rankBruteValue))
                    {
                        //setting rounded rank info
                        DiffusedRankInfo newLastBetInfo = new DiffusedRankInfo(roundededRank, rankBruteValue, lastBetInfo.CardsCount);
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
                    if (CardManager.SortedRanks.TryGetRankBruteValue(roundededRank, out rankBruteValue))
                    {
                        //setting rounded rank info
                        DiffusedRankInfo newRoundedBetInfo = new DiffusedRankInfo(roundededRank, rankBruteValue, NonSuccessiveRank.CardsCount);

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
                            DiffusedRankInfo newRounduprankInfo = new DiffusedRankInfo(existingRoundedUpRank.Rank, existingRoundedUpRank.RankBruteValue, NonSuccessiveRank.CardsCount);
                            DiffusedRankInfo newLastRankInfo = new DiffusedRankInfo(NonSuccessiveRank.Rank, NonSuccessiveRank.RankBruteValue, existingRoundedUpRank.CardsCount);
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
            else
            {
#if Log
                LogManager.LogError($"Failed to Round Up Rank{NonSuccessiveRank.Rank} bet is Successive and is not a maxed Bet!");
#endif
                return false;
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
                if (CardManager.SortedRanks.TryGetRankBruteValue(roundededRank, out rankBruteValue))
                {
                    //setting rounded rank info
                    DiffusedRankInfo newRoundedBetInfo = new DiffusedRankInfo(roundededRank, betToRoundUp.RankBruteValue, betToRoundUp.CardsCount);

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

    private static bool TwoRankBetRoundUp(byte[] bet, ref byte[] roundedUpBet, List<DiffusedRankInfo> diffusedBet)
    {
        int rankToRoundUpIndex = 0;

        DiffusedRankInfo firstRank = diffusedBet[0];
        DiffusedRankInfo secondRank = diffusedBet[1];
        bool twoHighestRanks = false;

        byte highestRank = CardManager.SortedRanks[CardManager.SortedRanks.Length - 1];
        byte SecondHighestRank = CardManager.SortedRanks[CardManager.SortedRanks.Length - 2];
        //checking if the two ranks that are played are two highest ranks
        twoHighestRanks = (diffusedBet.IsRankDiffused(highestRank) && diffusedBet.IsRankDiffused(SecondHighestRank));
        if (twoHighestRanks)
        {
            //<================= Second type of Rounding Up =====================>

            if (diffusedBet[1].CardsCount == CardManager.MaxRankCounter)
            {
#if Log
                LogManager.LogError("Failed to Round Up Bet" + string.Join(", ", bet) + "Invalid Bet Format!");
#endif
                return false;
            }
            //rounding up the secondRank because it is the lower value
            DiffusedRankInfo adjustedRank = new DiffusedRankInfo(secondRank.Rank, secondRank.RankBruteValue, (byte)(secondRank.CardsCount + 1));
            if (adjustedRank.CardsCount > firstRank.CardsCount)
            {
                //it is safe here to put only the rounded up rank
                diffusedBet.Clear();
            }
            else
                diffusedBet.Remove(secondRank);
            diffusedBet.Add(adjustedRank);
            roundedUpBet = diffusedBet.ToByteArray();
            return true;
            //<=========================================================>
        }
        else
        {
            byte roudededRank = 0;
            bool keepSecondRankCardsCount = false;
            //if ranks card count differ
            if (firstRank.CardsCount != secondRank.CardsCount)
            {
                if (secondRank.Rank != highestRank)
                {
                    //checking if the two rank are not successive to round up the lowest rank in value
                    if ((firstRank.RankBruteValue - secondRank.RankBruteValue) != 1)
                    {
                        rankToRoundUpIndex = 1;
                    }
                    else
                        keepSecondRankCardsCount = true;
                }
            }
            else
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
                if (CardManager.SortedRanks.TryGetRankBruteValue(roudededRank, out rankBruteValue))
                {
                    //setting rounded rank info
                    DiffusedRankInfo newLastBetInfo = new DiffusedRankInfo();
                    //making the second rank jump twice
                    newLastBetInfo.Rank = roudededRank;
                    newLastBetInfo.RankBruteValue = rankBruteValue;
                    if (keepSecondRankCardsCount)
                    {
                        newLastBetInfo.CardsCount = secondRank.CardsCount;
                        diffusedBet[1] = newLastBetInfo;
                    }
                    else
                    {
                        newLastBetInfo.CardsCount = RankToRoundUp.CardsCount;
                        diffusedBet[rankToRoundUpIndex] = newLastBetInfo;
                    }

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

    private static bool OneRankBetRoundUp(byte[] roundedUpBet, List<DiffusedRankInfo> diffusedBet)
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

    private static bool RoundUpMaxedBet(byte[] bet, ref byte[] roundedUpBet, int dealtCardsCount, int BetTotalCardsCount, List<DiffusedRankInfo> diffusedBet)
    {
        int NonLockedRanksCounter = 0;
        //first need to check if there are any Non Locked Ranks
        if (diffusedBet.IncludeNonLockedRank(out NonLockedRanksCounter))
        {
            //at this point if Non Locked rank Exist there should only one and  position should be the Last
            if (NonLockedRanksCounter != 1 || diffusedBet[diffusedBet.Count - 1].CardsCount == CardManager.MaxRankCounter)
            {
#if Log
                LogManager.Log("Failed to Round Up Bet" + string.Join(",", bet) + "In Valid BetFormat", Color.red, LogManager.Validators);
#endif
                return false;
            }
            //adding a card of the NonLockedRank to bet
            DiffusedRankInfo nonLockedRank = diffusedBet[diffusedBet.Count - 1];
            DiffusedRankInfo adjustedNonLockedRank = new DiffusedRankInfo(nonLockedRank.Rank, nonLockedRank.RankBruteValue, (byte)(nonLockedRank.CardsCount + 1));
            diffusedBet.Remove(nonLockedRank);
            diffusedBet.Add(adjustedNonLockedRank);
            //sorting is need nonLoked rank might become Locked after adding a card
            diffusedBet.SortDiffusedBet();
            roundedUpBet = diffusedBet.ToByteArray();
            return true;
        }
        // if bet only Contains Locked ranks then the diffrence of last bet total count and Total Dealt need to Be>=2
        else
        {
            if ((dealtCardsCount - BetTotalCardsCount) < 2)
            {
#if Log
                LogManager.Log("Failed to Round Up Bet" + string.Join(",", bet) + " is already Maxed Out !", Color.yellow, LogManager.Validators);
#endif
                return false;
            }
            //grabbing and unused rank to add to the bet
            byte rank = 0;
            int bruteValue = 0;
            bool rankisFound = false;
            for (int index = 0; index < CardManager.SortedRanks.Length; index++)
            {
                rank = CardManager.SortedRanks[index];
                if (!diffusedBet.IsRankDiffused(rank))
                {
                    rankisFound = true;
                    bruteValue = index + 1;
                    break;
                }
            }
            //if No ranks found then somthing is wrong
            if (!rankisFound)
            {
#if Log
                LogManager.LogError("Failed to Round Up Bet" + string.Join(", ", bet) + "All Ranks Are used! and bet is not Maxed!");
#endif
                return false;
            }
            DiffusedRankInfo diffusedRankInfo = new DiffusedRankInfo(rank, bruteValue, 2);

            //adding rank to bet no need to sort here
            diffusedBet.Add(diffusedRankInfo);
            roundedUpBet = diffusedBet.ToByteArray();
            return true;
        }
    }

    private static bool LaunchBet(ref byte[] roundedUpBet, int dealtCardsCount)
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
}