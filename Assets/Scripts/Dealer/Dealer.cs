using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dealer 
{
    private IDealerBehaviour _dealerBehaviour;
    public List<ICard> DeckOfCards { get => _dealerBehaviour.DeckOfCards; }
    public Dealer (NetworkRunner _runner)
    {
        if (_runner.GameMode == GameMode.Single)
            _dealerBehaviour = new OfflineDealerBehaviour(_runner);
        else
            _dealerBehaviour = new OnlineDealerBehaviour(_runner);
    }
}
