#define UsingUnityTest
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SinglePeerBase
{
    protected const byte BeloteDeckSize = 32;
    protected const byte StandartDeckSize = 52;
    protected const byte StandardSuitsNumber = 4;
    private readonly byte[] _beloteCardRanks = { 7, 8, 9, 11, 12, 13, 10, 1 };
    protected CardInfo[] FakeDeck = new CardInfo[BeloteDeckSize];
    protected byte MaxPlayerCards;
    protected FakePlayer[] FakePlayers;
    private TestCoroutineRunner _coroutineRunner;
#if UsingUnityTest
    [UnitySetUp]
    [Timeout(600000)]
    public IEnumerator MultiPeerSetup()
    {
        yield return new EnterPlayMode();
    }
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Debug.Log("Test Completed !");
        yield return new ExitPlayMode();
    }
#endif

    /// <summary>
    /// Create Players and Assign random Card Counters
    /// </summary>
    /// <param name="playerCount"></param>
    protected void CreateFakePlayers(int playerCount)
    {
        if (playerCount < 2 || playerCount > 8)
        {
            Debug.LogError($"Player Count is not Valid playerCount = {playerCount}");
            return;
        }
        MaxPlayerCards = SetMaxPlayerCards((byte)playerCount);
        FakePlayers = new FakePlayer[playerCount];
        FakePlayer player;
        string playerID;
        for (int index = 0; index < playerCount; index++)
        {
            player = new FakePlayer();
            playerID = Guid.NewGuid().ToString();
            player.SetID(playerID);
            player.SetMaxCards(MaxPlayerCards);
            player.SetCardCounter((byte)UnityEngine.Random.Range(1, MaxPlayerCards + 1));
            player.SetUpPlayerHand();
            FakePlayers[index] = player;
        }
    }

    protected void FillFakeDeck()
    {
        CardInfo card;
        byte CardID = 1;
        int arrayIndex = 0;
        for (byte suitIndex = 1; suitIndex < (StandardSuitsNumber+1); suitIndex++)
        {
            for (byte rankIndex = 0; rankIndex < _beloteCardRanks.Length; rankIndex++)
            {
                card = new CardInfo()
                {
                    Rank = _beloteCardRanks[rankIndex],
                    ID = CardID++,
                    Suit = (CardSuit)suitIndex,
                    IsValid = true
                };

                FakeDeck[arrayIndex++] = card;
            }
        }
    }

    protected byte SetMaxPlayerCards(byte playerNumber)
    {
        byte playerCards = 1;
        while ((BeloteDeckSize - (playerCards * playerNumber) > 0))
        {
            playerCards++;
        }
        return (byte)(playerCards - 1);
    }

    protected void LogDeck(CardInfo[] deck, string header)
    {
        for (int index = 0; index < deck.Length; index++)
            LogManager.Log(header + "  " + deck[index],Color.green,LogManager.ValueInformationLog);
    }

    #region protected bool methods

    /// <summary>
    /// returns true is the provided Deck is a Belote Deck with an Original Size (32) and the Cards within are valid
    /// </summary>
    /// <param name="deck"></param>
    /// <returns></returns>
    protected bool IsAValidBeloteDeckWithAStandartSize(CardInfo[] deck)
    {
        if (deck.Length != BeloteDeckSize)
        {
            Debug.LogError($"Deck Size {deck.Length} does not meet to Belote Deck Size which is {BeloteDeckSize} ");
            return false;
        }
        bool isValid = true;

        for (int index = 0; index < BeloteDeckSize; index++)
        {
            if (!IsCardValid(FakeDeck[index]))
                return false;
        }
        return isValid;
    }

    /// <summary>
    /// checks if a card a Card is Valid as a Belote Card
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    protected bool IsCardValid(CardInfo card)
    {
        if (!card.IsValid) return false;
        if (card.ID <= 0) return false;
        if (!_beloteCardRanks.Contains(card.Rank)) return false;
        return true;
    }

    protected bool ArePlayersValid(FakePlayer[] players)
    {
        if (players == null) return false;
        if (players.Length == 0) return false;
        bool allPlayersValid = true;
        for (int index = 0; index < players.Length; index++)
        {
            if (!IsPlayerValid(players[index]))
            {
                return false;
            }
        }
        return allPlayersValid;
    }

    protected bool IsPlayerValid(FakePlayer player)
    {
        if (player == null) return false;
        if (string.IsNullOrEmpty(player.ID)) return false;
        if (player.MaxCards == 0) return false;
        if (player.CardsToDealCounter > player.MaxCards) return false;
        if (player.PlayerHand == null) return false;
        return true;
    }

    /// <summary>
    /// true is the all player hands are valid Belote hands and are dealt all Cards that they suppose to get
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    protected bool IsDealingValid(FakePlayer[] players)
    {
        if (players == null) return false;
        if (players.Length == 0) return false;
        bool IsdealingValid = true;
        FakePlayer player;
        for (int index = 0; index < players.Length; index++)
        {
            player = players[index];
            if (player.CardsToDealCounter != player.PlayerHand.Length) return false;
            if (!IsPlayerHandValid(player.PlayerHand)) return false;
        }
        return IsdealingValid;
    }

    /// <summary>
    /// true if every card in his hand is a Valid Belote card
    /// </summary>
    /// <param name="playerHand"></param>
    /// <returns></returns>
    protected bool IsPlayerHandValid(CardInfo[] playerHand)
    {
        for (int index = 0; index < playerHand.Length; index++)
        {
            if (!IsCardValid(playerHand[index]))
                return false;
        }
        return true;
    }

    protected bool IsDeckShuffled(CardInfo[] refrenceDeck, CardInfo[] shuffledDeck)
    {
        if (refrenceDeck == null || shuffledDeck == null) return false;
        if (refrenceDeck.Length == 0 || shuffledDeck.Length == 0) return false;
        if (refrenceDeck.Length != shuffledDeck.Length) return false;
        int SameCardPositionsCounter = 0;
        for (int index = 0; index < refrenceDeck.Length; index++)
        {
            if (Extention.AreSameCard(refrenceDeck[index], shuffledDeck[index]))
                SameCardPositionsCounter++;
        }
        Debug.Log($"Same Cards Position Counter = {SameCardPositionsCounter}");
        return SameCardPositionsCounter < refrenceDeck.Length;
    }

    /// <summary>
    /// true if the deck is standart 52 deck that includes all 4 suits
    /// </summary>
    /// <param name="Deck"></param>
    /// <returns></returns>
    protected bool IsaStandartDeck(CardInfo[] Deck)
    {
        if (Deck.IsNotInitialized() || Deck.ValidCardsCount() != StandartDeckSize || Deck.IsEmpty()) return false;

        byte maxIndex = CardManager.STANDARD_DECK_SUIT_SIZE + 1;
        //checking ranks existense in deck
        for (byte index = 1; index < maxIndex; index++)
        {
            if (!Deck.ContainsRank(index) || Deck.DuplicateCounter(index) != StandardSuitsNumber)
            {
                return false;
            }
        }
        //checking all four suits exist in deck
        if (!AllStandartSuitsExistInDeck(Deck, DeckType.Standard))
            return false;
        return true;
    }

    protected bool IsaStandartBeloteDeck(CardInfo[] Deck)
    {
        if (Deck.IsNotInitialized() || Deck.ValidCardsCount() != BeloteDeckSize || Deck.IsEmpty()) return false;

        //checking ranks existense in deck
        for (byte index = 0; index < _beloteCardRanks.Length; index++)
        {
            byte rank = _beloteCardRanks[index];
            if (!Deck.ContainsRank(rank) || Deck.DuplicateCounter(rank) != StandardSuitsNumber)
            {
                return false;
            }
        }
        //checking all four suits exist in deck
        if (!AllStandartSuitsExistInDeck(Deck, DeckType.Belote))
            return false;
        return true;
    }

    protected bool AllStandartSuitsExistInDeck(CardInfo[] Deck, DeckType deckType)
    {
        //setting suit number
        byte SuitRanksNumber = 0;
        switch (deckType)
        {
            case DeckType.Standard: SuitRanksNumber = CardManager.STANDARD_DECK_SUIT_SIZE; break;
            case DeckType.Belote: SuitRanksNumber = CardManager.BELOTE_DECK_SUIT_SIZE; break;
        }
        int SuitCounter;
        for (byte suitIndex = 1; suitIndex < (StandardSuitsNumber+1); suitIndex++)
        {
            SuitCounter = 0;
            for (byte index = 0; index < Deck.Length; index++)
            {
                if (Deck[index].Suit == (CardSuit)suitIndex)
                    ++SuitCounter;
            }
            if (SuitCounter != SuitRanksNumber)
            {
                return false;
            }
        }
        return true;
    }

    #endregion protected bool methods
    #region protected wrappers
    /// <summary>
    /// wrapper for a start coroutine
    /// </summary>
    /// <param name="routine"></param>
    /// <returns></returns>
    protected Coroutine StartRoutine(IEnumerator routine)
    {
        if (_coroutineRunner == null)
        {
            var runerGo = new GameObject("TestCoroutineRunner");
            _coroutineRunner = runerGo.AddComponent<TestCoroutineRunner>();
        }
        return _coroutineRunner.StartCoroutine(routine);
    }
    /// <summary>
    /// wrapper for StopCoroutine
    /// </summary>
    /// <param name="routineCash"></param>
    protected void StopRoutine(Coroutine routineCash)
    {
        if (_coroutineRunner == null)
        {
            var runerGo = new GameObject();
            runerGo.AddComponent<TestCoroutineRunner>();
            _coroutineRunner = _coroutineRunner.GetComponent<TestCoroutineRunner>();
        }
        _coroutineRunner.StopCoroutine(routineCash);
    }
    #endregion
}
public class TestCoroutineRunner : MonoBehaviour
{

}