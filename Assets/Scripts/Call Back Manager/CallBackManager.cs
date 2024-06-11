using System;
using System.Collections.Generic;

public class CallBackManager
{
    private Queue<Action> _callbacks = new Queue<Action>();
    private bool _isReady = false;

    public void EnqueueOrExecute(Action callback)
    {
        if (_isReady)
        {
            callback?.Invoke();
        }
        else
        {
            _callbacks.Enqueue(callback);
        }
    }

    public void SetReady(bool ready)
    {
        _isReady = ready;
        if (_isReady)
        {
            DequeueAll();
        }
    }

    private void DequeueAll()
    {
        while (_callbacks.Count > 0)
        {
            _callbacks.Dequeue()?.Invoke();
        }
    }
}
