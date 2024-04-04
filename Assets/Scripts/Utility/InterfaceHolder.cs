



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

}