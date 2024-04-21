using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CardManagerTest : SinglePeerBase
{

    private const int MaxSuitsNumber = 8;
    private const string Belote = "Belote";
    private const string Standard = "Standart";
    private const string Custom = "Custom";

    [Test]
    public void StandardDeckCreationSizeTest()
    {
        StandardSizeDeckCheck(DeckType.Standard,
                              StandardSuitsNumber,
                              Standard, CardManager.STANDARD_DECK_SUIT_SIZE);
    }

    [UnityTest]
    public IEnumerator StandardDeckCreationAllSizeTest()
    {
        yield return DeckSizeCheckCoroutine(DeckType.Standard,
                                            Standard,
                                            CardManager.STANDARD_DECK_SUIT_SIZE);
    }

    [Test]
    public void BeloteDeckCreationStandardSizeTest()
    {
        StandardSizeDeckCheck(DeckType.Belote,
                              StandardSuitsNumber,
                              Belote,
                              CardManager.BELOTE_DECK_SUIT_SIZE);
    }
    [UnityTest]
    public IEnumerator BeloteDeckCreationAllSizeTest()
    {
        yield return DeckSizeCheckCoroutine(DeckType.Belote,
                                            Belote,
                                            CardManager.BELOTE_DECK_SUIT_SIZE);
    }

    [Test]
    public void CustomDeckCreationTest()
    {
        // Use the Assert class to test conditions
    }


















    #region private Routines
    private IEnumerator DeckSizeCheckCoroutine(DeckType decktype, string LogHeader, byte SuitSize)
    {
        Assert.IsNull(CardManager.Deck);

        for (int SuitIndex = 2; SuitIndex < (MaxSuitsNumber + 1); SuitIndex++)
        {
            //standart deck variables
            DeckInfo standartDeckInfo = new DeckInfo();
            standartDeckInfo.DeckType = decktype;
            standartDeckInfo.SuitsNumber = (byte)SuitIndex;
            CardManager.Init(standartDeckInfo);
            Assert.NotNull(CardManager.Deck);
            yield return null;

            //spitting the dick out 
            LogDeck(CardManager.Deck, $"{LogHeader} Deck with Suits Number {SuitIndex}");

            //cheking if the Deck size is what it is xD expected to be (stack rabbi ysabrou w barra ) 
            int predeterminedDeckSize = SuitSize * standartDeckInfo.SuitsNumber;
            int actualDeckSize = CardManager.Deck.CardsCount();
            Assert.AreEqual(predeterminedDeckSize, actualDeckSize);
#if Log
            Debug.Log($" suits Index {SuitIndex}, {LogHeader} Deck Size{actualDeckSize}");
#endif

            //ressetting the Deck
            CardManager.Reset();
            yield return null;
            Assert.IsTrue(CardManager.Deck.IsCardsArrayEmpty());

            //taking a break
            yield return new WaitForSeconds(1);
        }
    }
    #endregion



    #region private methods
    private void StandardSizeDeckCheck(DeckType deck, int suitSize, string LogHeader, byte SuitSize)
    {
        Assert.IsNull(CardManager.Deck);
        //standart deck variables
        DeckInfo standartDeckInfo = new DeckInfo();
        standartDeckInfo.DeckType = deck;
        standartDeckInfo.SuitsNumber = (byte)suitSize;

        CardManager.Init(standartDeckInfo);

        Assert.NotNull(CardManager.Deck);
        //cheking if the Deck size is what it is xD expected to be 
        int predeterminedDeckSize = SuitSize * standartDeckInfo.SuitsNumber;
        int actualDeckSize = CardManager.Deck.CardsCount();
        Assert.AreEqual(predeterminedDeckSize, actualDeckSize);
#if Log
        Debug.Log($"{LogHeader} Deck Size{actualDeckSize}");
#endif
        LogDeck(CardManager.Deck, $"{LogHeader} Deck");
    }
    #endregion

}
