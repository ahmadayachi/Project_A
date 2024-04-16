using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MaxPlayerCardsTest
{

    private const byte _beloteDeckSize = 32;
    private byte _maxPlayerCards;
    [Test]
    public void MaxPlayerCardsTestSimplePasses()
    {
        byte playerNumber = 9;
        _maxPlayerCards = SetMaxPlayerCards(playerNumber);
        Assert.AreEqual(3, _maxPlayerCards);
    }

    #region private methods
    private byte SetMaxPlayerCards(byte playerNumber)
    {
        byte playerCards = 1;
        while ((_beloteDeckSize - (playerCards * playerNumber) > 0))
        {
            playerCards++;
        }
        return (byte)(playerCards - 1);
    }
    #endregion
}
