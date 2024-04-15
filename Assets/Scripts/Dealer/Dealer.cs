
using UnityEngine;

public class Dealer : State
{
    //private IDealerBehaviour _dealerBehaviour;
    //public List<ICard> DeckOfCards { get => _dealerBehaviour.DeckOfCards; }
    //public Dealer (NetworkRunner _runner)
    //{
    //    if (_runner.GameMode == GameMode.Single)
    //        _dealerBehaviour = new OfflineDealerBehaviour(_runner);
    //    else
    //        _dealerBehaviour = new OnlineDealerBehaviour(_runner);
    //}


    public override void Start<T>(T arg)
    {
        if (!Extention.AreSameType<T, DealerArguments>(arg))
        {
#if Log
            Debug.LogError("wrong argument passed !");
#endif
            return;
        }

    }
    public override void ForceEnd()
    {
        throw new System.NotImplementedException();
    }

}
