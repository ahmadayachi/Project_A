using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetHandler 
{
    private IValidator _validatorChain;
    private IValidator _oneCardValidator;
    private IValidator _bettingCountValidator;
    private IValidator _betValidator;
    public BetHandler()
    {
        // setting up validators
        var oneCardValidator = new OneCardValidator();
        var totalBettingCountValidator = new TotalbettingCountValidator();
        var betValidator = new BetValidator();

        // Chaining validators
        oneCardValidator.Next = totalBettingCountValidator;
        totalBettingCountValidator.Next = betValidator;

        //setting fields 
        _oneCardValidator = oneCardValidator;
        _bettingCountValidator = totalBettingCountValidator;
        _betValidator = betValidator;
     
        // Seting the start of the chain
        _validatorChain = oneCardValidator;      
    }
    /// <summary>
    /// validate Bet and chaining all validation
    /// </summary>
    /// <param name="bet"></param>
    /// <param name="chain"></param>
    /// <returns></returns>
    public bool ChainValidateBet(ValidatorArguments Args)
    {
        return _validatorChain.Validate(Args);
    }

    public bool ValidateOneCardBet(ValidatorArguments Args)
    {
        return _oneCardValidator.Validate(Args);
    }
    public bool ValidateTotalBettingCount(ValidatorArguments Args)
    {
        return _bettingCountValidator.Validate(Args);
    }
    public bool ValidateBet(ValidatorArguments Args)
    {
        return _betValidator.Validate(Args);
    }
}

public abstract class ValidatorBase
{
    protected bool ValidArguments(ValidatorArguments arguments)
    {
        return ((!arguments.CurrentBet.IsNullOrEmpty())||(!arguments.PreviousBet.IsNullOrEmpty()));
    }
    /// <summary>
    /// true if Rank to compare is higher in value 
    /// </summary>
    /// <param name="Rank"></param>
    /// <param name="RankToCompare"></param>
    /// <returns></returns>
    protected bool RankIsHigherInValue(byte Rank, byte RankToCompare)
    {
        int rankValue = 0;
        int rankToCompareValue = 0;
        if (!CardManager.SortedRanks.TryGetRankValue(Rank, out rankValue) || !CardManager.SortedRanks.TryGetRankValue(RankToCompare, out rankToCompareValue))
        {
#if Log
            LogManager.LogError($" Failed to fetch Rank value ! Rank = {Rank} Rank to compare {rankToCompareValue}");
#endif
            return false;
        }
        return rankValue < rankToCompareValue;
    }
}
public class OneCardValidator : IValidator
{
    public IValidator Next { get ; set ; }

    public bool Validate(ValidatorArguments args)
    {
        // returning if null args
        if(args.CurrentBet==null || args.PreviousBet==null)
        {
            return false;
        }
        // returning if previous bet count > 1 
        if (args.PreviousBet.ValidCardsCount() > 1)
        {
            return false;
        }
        if (!args.PreviousBet.IsEmpty())
        {

        }
        return true;
    }
}
public class TotalbettingCountValidator : IValidator
{
    public IValidator Next { get ; set ; }

    public bool Validate(ValidatorArguments args)
    {
        throw new System.NotImplementedException();
    }
}
public class BetValidator : IValidator
{
    public IValidator Next { get ; set ;}

    public bool Validate(ValidatorArguments args)
    {
        throw new System.NotImplementedException();
    }
}