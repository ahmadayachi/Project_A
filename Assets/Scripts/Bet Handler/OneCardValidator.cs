using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneCardValidator : ValidatorBase, IValidator
{
    public IValidator Next { get; set; }

    public bool Validate(ValidatorArguments args)
    {
        // returning if null or empty Bet args
        if (!ValidBetArgs(args))
        {
#if Log
            LogManager.Log(LevelOne + ArgsNotValid, Color.yellow, LogManager.Validators);
#endif
            return false;
        }

        var CurrentBetCount = args.CurrentBet.ValidCardsCount();
        var PreviousBetCount = args.PreviousBet.ValidCardsCount();

        //bet not Valid if previous have more cards count
        if (CurrentBetCount == 1 && PreviousBetCount > 1)
        {
#if Log
            LogManager.Log(LevelOne + CurrentBetIsSmaller, Color.yellow, LogManager.Validators);
#endif
            return false;
        }

        if (CurrentBetCount == 1 && PreviousBetCount <= 1)
        {
            //if no previous bet , current bet should be valid
            if (PreviousBetCount == 0)
            {
#if Log
                LogManager.Log(LevelOne + BetPassValidation+Bet+string.Join(",",args.CurrentBet), Color.magenta, LogManager.Validators);
#endif
                return true;
            }

            byte previousBet = args.PreviousBet[0];
            byte currentBet = args.CurrentBet[0];
            bool currentRankIsHigherInValue = IsRankHigherInValue(previousBet, currentBet);

#if Log
            if (currentRankIsHigherInValue)
                LogManager.Log(LevelOne + BetPassValidation + Bet + string.Join(",", args.CurrentBet), Color.magenta, LogManager.Validators);
            else
                LogManager.Log(LevelOne + "Current Bet Must Be higher In value", Color.magenta, LogManager.Validators);

#endif             
            // if Current Rank is Higher then Bet is Valid
            return currentRankIsHigherInValue;
        }

        // if see no prob  pass responceability to next validator
        return Next != null ? Next.Validate(args) : true;
    }
}