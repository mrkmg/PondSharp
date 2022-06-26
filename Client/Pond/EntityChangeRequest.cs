using System.Runtime.InteropServices;

namespace PondSharp.Client.Pond;

[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 16)]
internal struct EntityChangeRequest : IResettable
{
    private enum EntityChangeRequestType
    {
        None = 0,
        Position = 1,
        Color = 2,
    }
        
    [FieldOffset(0)] private int EntityId;
    [FieldOffset(4)] private EntityChangeRequestType Type;
        
    [FieldOffset(8)] private int X;
    [FieldOffset(12)] private int Y;
        
    [FieldOffset(8)] private int Color;

    public void Reset()
    {
        Type = EntityChangeRequestType.None;
    }

    public void ForColor(int entityId, int color)
    {
        EntityId = entityId;
        Type = EntityChangeRequestType.Color;
        Color = color;
    }
        
    public void ForPosition(int entityId, int x, int y)
    {
        EntityId = entityId;
        Type = EntityChangeRequestType.Position;
        X = x;
        Y = y;
    }
}