



using Fusion;
using System;
using System.Collections.Generic;

#region Interfaces
#region Player stuff 
public interface IPlayerBehaviour
{

}
public interface IPlayer:ICardReceiver
{
    string Name {get;}
    void SetPlayerName(string playerName);
    string ID {get;}
    void SetPlayerID(string playerID);
    void SetCardCounter(byte cardCounter);
    CardInfo[] PlayerHand { get;}
    void ClearHand();
}
public interface ICardReceiver
{
    /// <summary>
    /// if true player cant/wont have cards or play 
    /// </summary>
    NetworkBool IsPlayerOut {get;}
    /// <summary>
    /// how much Cards to deal 
    /// </summary>
    byte CardsCounter { get;}
    void AddCard(CardInfo card);
}
#endregion

#region Card stuff  
public interface ICard : ICardInfo
{
    ICardUI CardUI { get; }
    void SetRank(byte rank);
    void SetID(byte id);
    void SetSuite(CardSuit suite);
    void Enable();
    void Disable();
    CardInfo ToCardInfo();
}
public interface ICardInfo
{
    byte Rank { get; }
    byte ID { get; }
    CardSuit Suite { get; }
}
public interface ICardUI
{

}
public interface ICardBehaviour
{

}
public struct CardInfo
{
    public byte Rank;
    public byte ID;
    public CardSuit Suit;
    public bool IsValid;
    public override string ToString()
    {
        return $"Ivalid {IsValid} ID: {ID}, Rank: {Rank}, Suit: {Suit}";
    }
}
public enum CardSuit
{
    S,
    D,
    H,
    C
}
#endregion
public interface IDealerBehaviour
{
    List<ICard> DeckOfCards { get; set; }
}

public interface IGameModeBehaviour
{

}
public interface IState<T> where T : struct
{
    void Start(T arg);
    void ForceEnd();
}
#endregion


#region Structs
public struct DealerArguments
{
    public CardInfo[] DeckToDeal;
    public ICardReceiver[] Players;
    public Action OnDealerStateEnds;
}

#endregion


#region enums
public enum GameState
{
    GameStarted,
    GameOver
}
public enum RoundState
{
    /// <summary>
/// the player that plays when a round just started, this player will have fewer choices to play 
/// </summary>
    FirstPlayerTurn,
    /// <summary>
    /// when a player passes the turn 
    /// </summary>
    PassTurn, 
    /// <summary>
    /// a normal player turn, were all choices are available
    /// </summary>
    PlayerTurn,
    /// <summary>
    /// Round ended , after check if game is over else just start a new round 
    /// </summary>
    RoundOver
}
#endregion