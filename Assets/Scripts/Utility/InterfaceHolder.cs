



using System.Collections.Generic;

#region Interfaces
#region Player stuff 
public interface IPlayerBehaviour
{

}
public interface IPlayer
{

}
#endregion

#region Card stuff  
public interface ICard
{
    byte Rank { get; }
    byte ID { get; }
    CardSuite Suite { get; }
    ICardUI CardUI { get; }
    void SetRank(byte rank);
    void SetID(byte id);
    void SetSuite(CardSuite suite);
    void Enable();
    void Disable();
}
public interface ICardUI
{

}
public interface ICardBehaviour
{

}
public struct CardIdentity
{
    public byte Rank;
    public byte ID;
    public CardSuite Suite;
}
public enum CardSuite
{
    S = 1,
    D = 2,
    H = 3,
    C = 4
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

}

#endregion


#region enums
public enum GameState
{
    GameStarted=1,
    GameOver=2
}
public enum RoundState
{
    FirstPlayerTurn=1,
    PassTurn=2,
    PlayerTurn=3,
    RoundOver=4
}

#endregion