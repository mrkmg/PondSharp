using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
#pragma warning disable 1591
    public abstract class Engine
    {
        public int MinX { get; protected set; }
        public int MaxX { get; protected set; }
        public int MinY { get; protected set; }
        public int MaxY { get; protected set; }
        
        public abstract IEnumerable<IEntity> Entities { get; }
        public abstract IEntity GetEntity(int entityId);
        public abstract IEntity GetEntityAt(int x, int y);

        public abstract IEnumerable<IEntity> GetVisibleEntities(IEntity entity);

        protected static void DoTick(Entity e)
        {
            e.DidMoveThisTick = false;
            e.DoTick();
        }

        protected static void WasCreated(Entity e) => e.WasCreated();
        protected static void WasDestroyed(Entity e) => e.WasDestroyed();
        protected void InitializeEntity(Entity e, EntityInitialization initialization) =>
            e.Initialize(this, initialization);
        
        #region Movement
        
        public virtual bool CanMoveTo(IEntity entity, int x, int y) => true;
        public bool MoveTo(int entityId, int x, int y) => MoveTo(GetEntity(entityId), x, y);
        public virtual bool MoveTo(IEntity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y)) return false;
            WriteEntityPosition(entity, x, y);
            return true;
        }
        protected void WriteEntityPosition(IEntity entity, int x, int y)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.X = x;
            aEntity.Y = y;
            OnEntityMoved(entity);
        }
        
        #endregion

        #region Color

        public virtual bool CanChangeColorTo(IEntity entity, int color) => true;
        public bool ChangeColorTo(int entityId, int color) => ChangeColorTo(GetEntity(entityId), color);
        public virtual bool ChangeColorTo(IEntity entity, int color)
        {
            if (!CanChangeColorTo(entity, color)) return false;
            WriteEntityColor(entity, color);
            return true;
        }
        protected void WriteEntityColor(IEntity entity, int color)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.Color = color;
            OnEntityColorChanged(entity);
        }

        #endregion

        #region ViewDistance


        public virtual bool CanChangeViewDistance(IEntity entity, int distance) => true;
        public bool ChangeViewDistance(int entityId, int distance) => ChangeViewDistance(GetEntity(entityId), distance);
        public bool ChangeViewDistance(IEntity entity, int distance)
        {
            if (!CanChangeViewDistance(entity, distance)) return false;
            WriteEntityViewDistance(entity, distance);
            return true;
        }
        private void WriteEntityViewDistance(IEntity entity, int distance)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.ViewDistance = distance;
            OnEntityViewDistanceChanged(entity);
        }

        #endregion
        

        #region Events

        public event EventHandler<(int, int)> EntityMoved;
        public event EventHandler<IEntity> EntityAdded;
        public event EventHandler<int> EntityColorChanged;
        public event EventHandler<int> EntityViewDistanceChanged;

        protected void OnEntityAdded(IEntity entity)
        {
            EntityAdded?.Invoke(this, entity);
        }

        protected void OnEntityMoved(IEntity entity)
        {
            EntityMoved?.Invoke(entity, (entity.X, entity.Y));
        }

        protected void OnEntityColorChanged(IEntity entity)
        {
            EntityColorChanged?.Invoke(entity, entity.Color);
        }

        protected void OnEntityViewDistanceChanged(IEntity entity)
        {
            EntityViewDistanceChanged?.Invoke(entity, entity.ViewDistance);
        }

        #endregion
        
        
    }

    public class EntityInitialization
    {
        public readonly int Id;
        public int X;
        public int Y;
        public int Color = 0xFFFFFF;
        public int ViewDistance;

        public EntityInitialization(int id)
        {
            Id = id;
        }
    }
#pragma warning restore 1591
}