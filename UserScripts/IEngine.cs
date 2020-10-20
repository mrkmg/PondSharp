using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
#pragma warning disable 1591
    public interface IEngine
    {
        int MinX { get; }
        int MaxX { get; }
        int MinY { get; }
        int MaxY { get; }
        IEnumerable<IAbstractEntity> Entities { get; }
        IAbstractEntity GetEntity(string entityId);
        bool CanMoveTo(IAbstractEntity entity, int x, int y);
        bool MoveTo(string entityId, int x, int y);
        bool MoveTo(IAbstractEntity entity, int x, int y);
        bool CanChangeColorTo(IAbstractEntity entity, int color);
        bool ChangeColorTo(string entityId, int color);
        bool ChangeColorTo(IAbstractEntity entity, int color);
        IEnumerable<IAbstractEntity> GetVisibleEntities(string entityId);
        IEnumerable<IAbstractEntity> GetVisibleEntities(IAbstractEntity entity);
        event EventHandler<(int, int)> EntityMoved;
        event EventHandler<IAbstractEntity> EntityAdded;
        event EventHandler<int> EntityColorChanged;
    }
#pragma warning restore 1591
}