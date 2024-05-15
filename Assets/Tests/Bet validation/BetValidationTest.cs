using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.tvOS;

public class BetValidationTest : SinglePeerBase
{
    private BetHandler _betHandler;
    private ValidatorArguments _validatorArgs;

    private byte[] _currentBet;
    private byte[] _previousBet;
    [Test]
    public void BetValidationsTest()
    {
        // setting up CardManager Deck
        StandardSizeDeckCheck(DeckType.Belote,
                              StandardSuitsNumber,
                              Belote,
                              CardManager.BELOTE_DECK_SUIT_SIZE);

        //creating a BetHandler 
        _betHandler = new BetHandler();

        //creating a bet args 
        _validatorArgs = new ValidatorArguments();

        //_currentBet = BetGenerator.GenerateMaxBet(9);
        //_currentBet = new byte[] { 7,7,7 };
        _previousBet = new byte[] {13,13,13,1,1};

        bool isrounded = BetGenerator.TryRoundUpBet(_previousBet, out _currentBet,30);

        _validatorArgs.CurrentBet = _currentBet;
        _validatorArgs.PreviousBet = _previousBet;
        _validatorArgs.DealtCardsNumber = 30;

        bool isBetValid = _betHandler.ChainValidateBet(_validatorArgs);
        Assert.IsTrue(isBetValid);
    }
    [Test]
    public void BetSortingTest()
    {
        // setting up CardManager Deck
        StandardSizeDeckCheck(DeckType.Belote,
                              StandardSuitsNumber,
                              Belote,
                              CardManager.BELOTE_DECK_SUIT_SIZE);
        _currentBet = new byte[] {7,7, 7,1, 1, 1, 1 };
        Dictionary<byte,byte> diffusedBet = new Dictionary<byte,byte>();
        Extention.BetDiffuserAlpha(_currentBet, diffusedBet,0);
        diffusedBet.SortBet();
        Debug.Log(string.Join(",", diffusedBet));
        _currentBet = diffusedBet.ToByteArray();
        Debug.Log(string.Join(",", _currentBet));

    }

    #region private methods

    //private void SetUpCardManager

    #endregion private methods
}