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
        IEnumerable<IEntity> Entities { get; }
        IEntity GetEntity(int entityId);
        bool CanMoveTo(IEntity entity, int x, int y);
        bool MoveTo(int entityId, int x, int y);
        bool MoveTo(IEntity entity, int x, int y);
        bool CanChangeColorTo(IEntity entity, int color);
        bool ChangeColorTo(int entityId, int color);
        bool ChangeColorTo(IEntity entity, int color);
        bool CanChangeViewDistance(IEntity entity, int distance);
        bool ChangeViewDistance(int entityId, int distance);
        bool ChangeViewDistance(IEntity entity, int distance);
        IEnumerable<IEntity> GetVisibleEntities(int entityId);
        IEnumerable<IEntity> GetVisibleEntities(IEntity entity);
        event EventHandler<(int, int)> EntityMoved;
        event EventHandler<IEntity> EntityAdded;
        event EventHandler<int> EntityColorChanged;
        event EventHandler<int> EntityViewDistanceChanged;
    }
#pragma warning restore 1591
}