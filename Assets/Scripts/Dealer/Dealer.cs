
using UnityEngine;

public class Dealer : State
{
    #region State    
    public override void Start<T>(T arg)
    {
        if (!Extention.AreSameType<T, DealerArguments>(arg))
        {
#if Log
            Debug.LogError("wrong argument passed !");
#endif
            return;
        }
        //do the do here ? 

    }
    public override void ForceEnd()
    {

    }
    #endregion
    private CardIdentity[] _dealtCards;
    private int _maxCardsToDeal;
    public Dealer()
    {

    }
   
    private void DealCards(DealerArguments arguments)
    {

    }
}
