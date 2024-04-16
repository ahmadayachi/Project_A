using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager :NetworkBehaviour
{
    #region Player Propertys 
    [Networked, Capacity(8)] public NetworkArray<NetworkObject> Clients { get; }
    private byte _playernumber;
    public byte PlayerNumber { get => _playernumber;}
    #endregion
    private Dealer _dealer;

    #region Dealer Setup
    private void Awake()
    {
       
    }
    #endregion
    private const int BeloteDeckSize = 32;
    private byte _maxPlayerCards;
    public byte MaxPlayerCards { get => _maxPlayerCards;}
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
        byte playerCards = 1;
        while ((BeloteDeckSize - (playerCards * PlayerNumber) > 0))
        {
            playerCards++;
        }
    }
}
