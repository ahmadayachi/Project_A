



using System.Collections.Generic;
using Unity.VisualScripting;

public interface IPlayerBehaviour
{

}
public interface IDealerBehaviour
{
    List<ICard> DeckOfCards { get; set; }
}
public interface ICard
{
    byte Rank { get;}
    byte ID { get; }
    string Suite { get;}
    void SetRank(byte rank);
    void SetID(byte id);
    void SetSuite(string suite);
}
public interface ICardBehaviour
{

}
public interface IGameModeBehaviour
{

}