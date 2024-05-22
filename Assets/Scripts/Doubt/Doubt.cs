using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doubt : State
{    
    private Coroutine _doubtingRoutine;
    private Func<DoubtState, IEnumerator> _onDoubtOverLogic;
    private Func<IEnumerator, Coroutine> _startRoutine;
    private Action<Coroutine> _stopRoutine;

    public Doubt(Func<DoubtState, IEnumerator> onDoubtOverLogic, Func<IEnumerator, Coroutine> startRoutine, Action<Coroutine> stopRoutine)
    {
        _onDoubtOverLogic = onDoubtOverLogic;
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
        //searching and spliting bet in two lists
        List<byte> correctBetRanks = new List<byte>();
        List<byte> wrongBetRanks = new List<byte>();
        foreach (byte Rank in args.Livebet)
        {
            if (args.DealtCards.Contains(Rank))
            {
                correctBetRanks.Add(Rank);
                args.DealtCards.Remove(Rank);
            }
            else
            {
                wrongBetRanks.Add(Rank);
            }
        }
        yield return null;
        //if the wrong Bet ranks is not empty then it is a Loss 
        DoubtState doubtState = wrongBetRanks.Count == 0 ? DoubtState.WinDoubt : DoubtState.LooseDoubt;
        //invoking further Logic 
        yield return _onDoubtOverLogic?.Invoke(doubtState);
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
