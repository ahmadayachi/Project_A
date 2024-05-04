using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalbettingCountValidator : ValidatorBase, IValidator
{
    public IValidator Next { get; set; }

    private List<byte> _allUsedRanksList = new List<byte>();

    public bool Validate(ValidatorArguments args)
    {
        // returning if null or empty Bet args
        if (!ValidBetArgs(args))
        {
#if Log
            LogManager.Log(LevelTwo + ArgsNotValid, Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }
        //dealt cards need to > 0
        if (args.dealtCardsNumber == 0)
        {
#if Log
            LogManager.LogError(LevelTwo + "ARGS dealt Cards are not valid ");
#endif
            return false;
        }

        var CurrentBetCount = args.CurrentBet.ValidCardsCount();
        var PreviousBetCount = args.PreviousBet.ValidCardsCount();

        // cant bet on less Cards then previous Bet
        if (CurrentBetCount < PreviousBetCount)
        {
            ValidationLogger(LevelTwo, false);
            return false;
        }

        // can bet on more Cards then that are dealt to players
        if (CurrentBetCount > args.dealtCardsNumber)
        {
            ValidationLogger(LevelTwo, false);
            return false;
        }

        // all betted Ranks counter should be Valid
        _allUsedRanksList.Clear();
        SetUpAllUsedRanksAlpha(args.CurrentBet, _allUsedRanksList, 0);
        if (!AllUsedRanksValid(_allUsedRanksList, args.CurrentBet))
        {
#if Log
            LogManager.Log(LevelTwo + "Used ranks in Current Bet are not Valid !", Color.yellow, LogManager.Validators);
#endif
            return false;
        }

        // if see no prob  pass responceability to next validator
        return Next != null ? Next.Validate(args) : true;
    }
}