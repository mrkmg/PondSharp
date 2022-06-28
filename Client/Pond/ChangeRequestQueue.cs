using System;
using System.Collections.Generic;

namespace PondSharp.Client.Pond;

internal interface IResettable
{
    void Reset();
}

internal sealed class ChangeRequestQueue<T> where T : IResettable
{
    private const int QueueLength = 2000;
    
    private int _index = -1;
    private readonly T[] _requests = new T[QueueLength];
    private readonly Action<IReadOnlyList<T>> _flushAction;
    
    public ChangeRequestQueue(Action<IReadOnlyList<T>> flushAction)
    {
        _flushAction = flushAction;
    }

    public void Reset()
    {
        for (var i = 0; i <= _index; i++) _requests[i].Reset();
        _index = -1;
    }
    
    public void Flush()
    {
        if (_index == -1) return;
        
        _flushAction(_requests);
        Reset();
    }

    public ref T Next()
    {
        if (_index + 1 >= QueueLength)
            Flush();
     
        _index++;
        return ref _requests[_index];
    }
}