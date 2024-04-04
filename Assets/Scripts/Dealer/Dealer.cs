using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dealer 
{
    private IDealerBehaviour _dealerBehaviour;
    public List<ICard> DeckOfCards { get => _dealerBehaviour.DeckOfCards; }
    public Dealer (GameMode gameMode)
    {
        if (gameMode == GameMode.Single)
            _dealerBehaviour = new OfflineDealerBehaviour();
        else
            _dealerBehaviour = new OnlineDealerBehaviour();
    }
}
