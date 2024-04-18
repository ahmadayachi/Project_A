using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SinglePeerBase
{
    protected const int BeloteDeckSize = 32;
    private readonly byte[] _beloteCardRanks = { 7, 8, 9, 11, 12, 13, 10, 1 };
    protected  CardInfo[] FakeDeck = new CardInfo[BeloteDeckSize];
    private const int SuitsNumber = 4;
    protected byte MaxPlayerCards;
    protected FakePlayer[] FakePlayers;
    //[UnitySetUp]
    //[Timeout(600000)]
    //public IEnumerator MultiPeerSetup()
    //{
    //    yield return new EnterPlayMode();
    //}
    //[UnityTearDown]
    //public IEnumerator TearDown()
    //{
    //    Debug.Log("Test Completed !");
    //    yield return new ExitPlayMode();
    //}
    protected void FillFakeDeck()
    {
        CardInfo card;
        byte CardID = 1;
        for(int suitIndex = 0; suitIndex < SuitsNumber; suitIndex++)
        {
            for(int rankIndex=0;rankIndex<_beloteCardRanks.Length; rankIndex++)
            {
                card = new CardInfo()
                {
                    Rank = _beloteCardRanks[rankIndex],
                    ID = CardID++,
                    Suit =(CardSuit)suitIndex,
                    IsValid = true
                };

                if (!FakeDeck.AddCard(card))
                {
#if Log
                    Debug.LogError("Failed to Add card to Deck in Deck Creation !");
#endif
                    return;
                }
            }
        }
    }
    /// <summary>
    /// returns true is the provided Deck is a Belote Deck and the Cards within are valid
    /// </summary>
    /// <param name="deck"></param>
    /// <returns></returns>
    protected bool IsAValidBeloteDeck(CardInfo[] deck)
    {
        if(deck.Length != BeloteDeckSize)
        {
            Debug.LogError($"Deck Size {deck.Length} does not meet to Belote Deck Size which is {BeloteDeckSize} ");
            return false;
        }
        bool isValid = true;

        for(int index = 0; index < BeloteDeckSize; index++)
        {
            if (!IsCardValid(FakeDeck[index]))
                return false;
        }
        return isValid;
    }
    protected bool IsCardValid(CardInfo card)
    {
        if (!card.IsValid) return false;
        if (card.ID <= 0) return false;
        if (!_beloteCardRanks.Contains(card.Rank)) return false;
        return true;
    }
    /// <summary>
    /// Create Players and Assign random Card Counters  
    /// </summary>
    /// <param name="playerCount"></param>
    protected void CreateFakePlayers(int playerCount)
    {
        if(playerCount<2 || playerCount>8)
        {
            Debug.LogError($"Player Count is not Valid playerCount = {playerCount}");
            return;
        }
        MaxPlayerCards = SetMaxPlayerCards((byte)playerCount);
        FakePlayers = new FakePlayer[playerCount];
        FakePlayer player;
        string playerID;
        for(int index = 0;index < playerCount; index++)
        {
            player = new FakePlayer();
            playerID = Guid.NewGuid().ToString();
            player.SetID(playerID);
            player.SetMaxCards(MaxPlayerCards);            
            player.SetCardCounter((byte)UnityEngine.Random.Range(1,MaxPlayerCards+1));
            player.SetUpPlayerHand();
            FakePlayers[index] = player;
        }
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
        if (player.CardsCounter > player.MaxCards) return false;
        if (player.PlayerHand == null) return false;
        return true;
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
    protected bool IsDealingValid(FakePlayer[] players)
    {
        if (players == null) return false;
        if (players.Length == 0) return false;
        bool IsdealingValid = true;
        FakePlayer player;
        for (int index = 0; index < players.Length; index++)
        {
            player = players[index];
            if (player.CardsCounter != player.PlayerHand.Length) return false;
            if (!IsPlayerHandValid(player.PlayerHand)) return false;
        }
        return IsdealingValid;
    }
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
        if(refrenceDeck == null||shuffledDeck==null) return false;
        if(refrenceDeck.Length==0 || shuffledDeck.Length==0) return false;
        if(refrenceDeck.Length != shuffledDeck.Length) return false;
        int SameCardPositionsCounter = 0;
        for (int index = 0;index < refrenceDeck.Length; index++)
        {
            if (Extention.AreSameCard(refrenceDeck[index], shuffledDeck[index]))
                SameCardPositionsCounter++;
        }
        Debug.Log($"Same Cards Position Counter = {SameCardPositionsCounter}");
        return SameCardPositionsCounter<refrenceDeck.Length;
    }
    protected void LogDeck(CardInfo[] deck,string header)
    {
        for(int index = 0;index<deck.Length;index++)
            Debug.Log(header+"  "+deck[index]);
    }
}
