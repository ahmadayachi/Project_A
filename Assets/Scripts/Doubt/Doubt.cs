using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doubt : State
{    
    private Coroutine _doubtingRoutine;
    private Action<DoubtState> _onDoubtLogic;
    private Func<IEnumerator, Coroutine> _startRoutine;
    private Action<Coroutine> _stopRoutine;

    public Doubt(Action<DoubtState> onDoubtLogic, Func<IEnumerator, Coroutine> startRoutine, Action<Coroutine> stopRoutine)
    {
        _onDoubtLogic = onDoubtLogic;
        _startRoutine = startRoutine;
        _stopRoutine = stopRoutine;
    }

    public override void Start<T>(T arg)
    {
        if (Extention.TryCastToStruct(arg, out DoubtStateArguments DoubtArgs))
        {
            if (_doubtingRoutine != null)
                _stopRoutine?.Invoke(_doubtingRoutine);
            _doubtingRoutine = _startRoutine?.Invoke(DoubtBet(DoubtArgs));
        }
        else
        {
#if Log
            LogManager.LogError("wrong Doubt argument passed !");
#endif
        }
    }
    public override void ForceEnd()
    {
        if (_doubtingRoutine != null)
        {
            _stopRoutine?.Invoke(_doubtingRoutine);
            _doubtingRoutine = null;
        }
#if Log
        LogManager.Log("Doubting is forced to Stop!", Color.yellow, LogManager.DealerLog);
#endif
    }

    private IEnumerator DoubtBet(DoubtStateArguments args)
    {
        //cheking for invalid args
        if (InvalidArgs(args))
        {
#if Log
            LogManager.LogError("Doubt Arguments are Invalid !");
#endif
            _doubtingRoutine = null;
            yield break;
        }
        // any wrong Cards in bet results in Loosing the Bet 
        DoubtState doubtState = DoubtState.WinDoubt;
        foreach (byte Rank in args.Livebet)
        {
            if (args.DealtCards.Contains(Rank))
            {
                args.DealtCards.Remove(Rank);
            }
            else
            {
                doubtState = DoubtState.LooseDoubt;
                break;
            }
        }
        yield return null;
        //invoking further Logic 
        _onDoubtLogic?.Invoke(doubtState);
        //reseting coroutine
        _doubtingRoutine = null;
    }
    private bool InvalidArgs(DoubtStateArguments args)
    {
        if(args.Livebet == null||args.Livebet.ValidCardsCount()==0 ||args.DealtCards==null||args.DealtCards.Count==0)
        {
            return true;
        }
        return false;
    }
}
