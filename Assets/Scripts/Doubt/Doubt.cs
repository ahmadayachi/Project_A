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
        //searching and spliting bet in two lists
        List<byte> correctBetRanks = new List<byte>();
        List<byte> wrongBetRanks = new List<byte>();
        foreach (byte Rank in args.Livebet)
        {
            if (!args.DealtCards.Contains(Rank))
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
        var doubtOverUI = new DoubtOverUIArguments(correctBetRanks, wrongBetRanks);

    }
}
