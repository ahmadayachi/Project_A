using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalBettingCountValidator : ValidatorBase, IValidator
{
    public IValidator Next { get; set; }
    private Dictionary<byte, byte> _currentBetPair = new Dictionary<byte, byte>();
    private Dictionary<byte, byte> _previousBetPair = new Dictionary<byte, byte>();

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
        if (args.DealtCardsNumber == 0)
        {
#if Log
            LogManager.LogError(LevelTwo + "ARGS dealt Cards are not valid ");
#endif
            return false;
        }

        int CurrentBetCount = args.CurrentBet.ValidCardsCount();
        int PreviousBetCount = args.PreviousBet.ValidCardsCount();

        // can bet on more Cards then that are dealt to players
        if (CurrentBetCount > args.DealtCardsNumber)
        {
#if Log
            LogManager.Log(LevelTwo + "Current Bet  Cards Count Cant be more than dealt Cards !" + Bet + string.Join(",", args.CurrentBet), Color.yellow, LogManager.Validators);
#endif
            return false;
        }

        // all betted Ranks counter should be Valid
        _currentBetPair.Clear();
        Extention.BetDiffuserAlpha(args.CurrentBet, _currentBetPair, 0);

        if (DiffusedBetRanksCounterNotValid(_currentBetPair))
        {
#if Log
            LogManager.Log(LevelTwo + "Used ranks in Current Bet are not Valid !" + Bet + string.Join(",", args.CurrentBet), Color.yellow, LogManager.Validators);
#endif
            return false;
        }

        if (CurrentBetCount < PreviousBetCount)
        {
            // diffusing previous bet here
            _previousBetPair.Clear();
            Extention.BetDiffuserAlpha(args.PreviousBet, _previousBetPair, 0);
            // cant bet on less Cards then previous Bet
            if (IsSmallerBetNotValid(_currentBetPair, _previousBetPair))
            {
#if Log
                LogManager.Log(LevelTwo + CurrentBetIsSmaller + Bet + string.Join(",", args.CurrentBet), Color.yellow, LogManager.Validators);
#endif
                return false;
            }
            else
            {
#if Log
                LogManager.Log(LevelTwo + BetPassValidation + Bet + string.Join(",", args.CurrentBet), Color.magenta, LogManager.Validators);
#endif
                return true;
            }
        }

        // if see no prob  pass responceability to next validator
        return Next != null ? Next.Validate(args) : true;
    }
}