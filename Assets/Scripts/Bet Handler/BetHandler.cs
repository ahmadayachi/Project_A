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
        var totalBettingCountValidator = new TotalBettingCountValidator();
        var bruteValueBetValidator = new BruteValueBetValidator();

        // Chaining validators
        oneCardValidator.Next = totalBettingCountValidator;
        totalBettingCountValidator.Next = bruteValueBetValidator;

        //setting fields 
        _oneCardValidator = oneCardValidator;
        _bettingCountValidator = totalBettingCountValidator;
        _betValidator = bruteValueBetValidator;
     
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

#if UNITY_EDITOR
    public bool LevelOne(ValidatorArguments Args)
    {
        return _oneCardValidator.Validate(Args);
    }
    public bool LevelTwo(ValidatorArguments Args)
    {
        return _bettingCountValidator.Validate(Args);
    }
    public bool LevelThree(ValidatorArguments Args)
    {
        return _betValidator.Validate(Args);
    }
#endif
}




