using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteValueBetValidator : ValidatorBase, IValidator
{
    public IValidator Next { get; set; }
    private Dictionary<byte, byte> _currentBetPair = new Dictionary<byte, byte>();
    private Dictionary<byte, byte> _previousBetPair = new Dictionary<byte, byte>();


    public bool Validate(ValidatorArguments args)
    {
        if (!ValidBetArgs(args))
        {
#if Log
            LogManager.Log(LevelThree + ArgsNotValid, Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        //diffusing bet array to a manageable dictionary
        _currentBetPair.Clear();
        BetDiffuserAlpha(args.CurrentBet, _currentBetPair, 0);

        //chekking if bet ranks have an invalid rank counter
        if (IsDiffusedBetNotValid(_currentBetPair))
        {
#if Log
            LogManager.Log(LevelThree + "Diffused bet not valid! ", Color.yellow, LogManager.ValueInformationLog);
#endif
            return false;
        }

        int CurrentBetCount = args.CurrentBet.ValidCardsCount();
        int PreviousBetCount = args.PreviousBet.ValidCardsCount();

        // can disable this chekc to make all current bets must be just higher in value not in cards number
        if (CurrentBetCount == PreviousBetCount)
        {
            //at this point need to diffuse the previous rank and compare it
            _previousBetPair.Clear();
            BetDiffuserAlpha(args.PreviousBet,_previousBetPair,0);

            //converting bets to brute value which consists of the true (value of rank +1) * (rank counter)
            int currentBetBruteValue = DiffusedDeckToBruteValue(_currentBetPair);
            int previousBetBruteValue = DiffusedDeckToBruteValue(_previousBetPair);
            //current Bet Brute Value should be Higher then Previous Bet
            if (currentBetBruteValue < previousBetBruteValue)
            {
#if Log
                LogManager.Log(LevelThree + $"Current bet Brute Value Is less then previous! Currenbet brute Value {currentBetBruteValue} previou bet Brute Value {previousBetBruteValue} ", Color.yellow, LogManager.ValueInformationLog);
#endif
                return false;
            }
        }

        // at this point current bet should be Valid and have a higher number of cards
#if Log
        LogManager.Log(LevelThree + BetPassValidation, Color.magenta, LogManager.Validators);
#endif
        return true;
    }
}