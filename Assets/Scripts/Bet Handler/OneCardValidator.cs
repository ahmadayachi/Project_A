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
            LogManager.Log("OneCardValidator ARGS are not valid ", Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        var CurrentBetCount = args.CurrentBet.ValidCardsCount();
        var PreviousBetCount = args.PreviousBet.ValidCardsCount();

        //bet not Valid if previous have more cards count
        if (CurrentBetCount == 1 && PreviousBetCount > 1) return false;

        if (CurrentBetCount == 1 && PreviousBetCount <= 1)
        {
            //if no previous bet , current bet should be valid
            if (PreviousBetCount == 0) return true;

            byte previousBet = args.PreviousBet[0];
            byte currentBet = args.CurrentBet[0];
            bool currentRankIsHigherInValue = IsRankHigherInValue(previousBet, currentBet);

            // if Current Rank is Higher then Bet is Valid
            return currentRankIsHigherInValue ? true : false;
        }

        // if see no prob  pass responceability to next validator
        return Next != null ? Next.Validate(args) : true;
    }
}