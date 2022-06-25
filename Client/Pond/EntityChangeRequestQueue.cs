using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PondSharp.Client.Pond;

internal sealed class EntityChangeRequestQueue
{
    private const int QueueLength = 2000;
    
    private int _index = -1;
    private EntityChangeRequest[] _requests = new EntityChangeRequest[QueueLength];
    public int Count => _index + 1;
    private readonly Action<IReadOnlyList<EntityChangeRequest>> _flushAction;
    
    public EntityChangeRequestQueue(Action<IReadOnlyList<EntityChangeRequest>> flushAction)
    {
        _flushAction = flushAction;
    }

    public void Reset()
    {
        for (var i = 0; i <= _index; i++) _requests[i].Type = EntityChangeRequestType.None;
        _index = -1;
    }
    
    public void MoveEntity(int entityId, int x, int y)
    {
        _index++;
        ref var changeReq = ref _requests[_index];
        changeReq.EntityId = entityId;
        changeReq.Type = EntityChangeRequestType.Position;
        changeReq.X = x;
        changeReq.Y = y;
        if (Count == QueueLength)
            Flush();
    }

    public void ChangeEntityColor(int entityId, int color)
    {
        _index++;
        ref var changeReq = ref _requests[_index];
        changeReq.EntityId = entityId;
        changeReq.Type = EntityChangeRequestType.Color;
        changeReq.Color = color;
        if (Count == QueueLength)
            Flush();
    }

    public void Flush()
    {
        if (_index == -1) return;
        
        _flushAction(_requests);
        Reset();
    }
}

    
public enum EntityChangeRequestType
{
    None = 0,
    Position = 1,
    Color = 2,
}
    
[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 16)]
public struct EntityChangeRequest
{
    [FieldOffset(0)] public int EntityId;
    [FieldOffset(4)] public EntityChangeRequestType Type;
        
    [FieldOffset(8)] public int X;
    [FieldOffset(12)] public int Y;
        
    [FieldOffset(8)] public int Color;
}