using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    public interface IAbstractEntity
    {
        string Id { get; }
        int X { get; }
        int Y { get; }
        int Color { get; }
        int ViewDistance { get; }
        IEnumerable<IAbstractEntity> VisibleEntities { get; }
        void Initialize(string id, IEngine engine, int x = 0, int y = 0, int color = 0xFFFFFF, int viewDistance = 0);
        bool CanMoveTo(int x, int y);
        bool MoveTo(int x, int y);
        bool CanChangeColorTo(int color);
        bool ChangeColor(int color);
        void OnCreated();
        void Tick();
    }
}