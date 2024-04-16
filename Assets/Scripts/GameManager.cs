using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    private Dealer _dealer;

    #region Deck props
    private const int BeloteDeckSize = 32;
    private byte _maxPlayerCards;
   /// <summary>
   /// The max amount of cards that can be dealt to a player, a player should be out if he carry more than this amount 
   /// </summary>
    public byte MaxPlayerCards { get => _maxPlayerCards;}
    #endregion

    #region Player Propertys 
    [Networked, Capacity(8)] public NetworkArray<NetworkObject> Clients { get; }
    private byte _playernumber;
    public byte PlayerNumber { get => _playernumber;}
    #endregion


    #region Dealer Setup

    #endregion

    public void SetNumberOfPlayers(int numberOfPlayers)
    {
        if(numberOfPlayers<=8 && numberOfPlayers>0)
        {
            _playernumber = (byte) numberOfPlayers;
#if Log
            Debug.Log($"Player number is Set !, Player Number = {_playernumber}");
#endif
        }
    }
    private void SetMaxPlayerCards()
    {
        if ( _playernumber == 0)
        {
#if Log
            Debug.LogError("player number need to be > 0 before setting the max player cards ");
#endif
            return;
        }
        byte playerCards = 1;
        while ((BeloteDeckSize - (playerCards * PlayerNumber) > 0))
        {
            playerCards++;
        }
        _maxPlayerCards = (byte)(playerCards-1);
    }
}
