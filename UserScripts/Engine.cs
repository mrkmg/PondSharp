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
        
        public abstract IEnumerable<Entity> Entities { get; }
        public abstract Entity GetEntity(int entityId);
        public abstract Entity GetEntityAt(int x, int y);
        
        public abstract bool DestroyEntity(Entity entity);
        public abstract bool CanCreateEntity<T>(Entity entity);
        public abstract T CreateEntity<T>(EntityOptions options) where T : Entity;

        public abstract IEnumerable<Entity> GetVisibleEntities(Entity entity);

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
        
        public virtual bool CanMoveTo(Entity entity, int x, int y) => true;
        public bool MoveTo(int entityId, int x, int y) => MoveTo(GetEntity(entityId), x, y);
        public virtual bool MoveTo(Entity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y)) return false;
            WriteEntityPosition(entity, x, y);
            return true;
        }
        protected void WriteEntityPosition(Entity entity, int x, int y)
        {
            entity.X = x;
            entity.Y = y;
            OnEntityMoved(entity);
        }
        
        #endregion

        #region Color

        public virtual bool CanChangeColorTo(Entity entity, int color) => true;
        public bool ChangeColorTo(int entityId, int color) => ChangeColorTo(GetEntity(entityId), color);
        public virtual bool ChangeColorTo(Entity entity, int color)
        {
            if (!CanChangeColorTo(entity, color)) return false;
            WriteEntityColor(entity, color);
            return true;
        }
        protected void WriteEntityColor(Entity entity, int color)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.Color = color;
            OnEntityColorChanged(entity);
        }

        #endregion

        #region ViewDistance
        
        public virtual bool CanChangeViewDistance(Entity entity, int distance) => true;
        public bool ChangeViewDistance(int entityId, int distance) => ChangeViewDistance(GetEntity(entityId), distance);
        public bool ChangeViewDistance(Entity entity, int distance)
        {
            if (!CanChangeViewDistance(entity, distance)) return false;
            WriteEntityViewDistance(entity, distance);
            return true;
        }
        private void WriteEntityViewDistance(Entity entity, int distance)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.ViewDistance = distance;
            OnEntityViewDistanceChanged(entity);
        }

        #endregion
        
        #region IsBlocking
        
        public virtual bool CanChangeIsBlocking(Entity entity) => false;
        public bool ChangeIsBlocking(int entityId, bool isBlocking) => ChangeIsBlocking(GetEntity(entityId), isBlocking);
        public bool ChangeIsBlocking(Entity entity, bool isBlocking)
        {
            if (!CanChangeIsBlocking(entity)) return false;
            WriteEntityIsBlocking(entity, isBlocking);
            return true;
        }
        private void WriteEntityIsBlocking(Entity entity, bool isBlocking)
        {
            if (!(entity is Entity aEntity)) return;
            aEntity.IsBlocking = isBlocking;
        }

        #endregion
        

        #region Events

        public event EventHandler<(int, int)> EntityMoved;
        public event EventHandler<Entity> EntityAdded;
        public event EventHandler<Entity> EntityRemoved;
        public event EventHandler<int> EntityColorChanged;
        public event EventHandler<int> EntityViewDistanceChanged;

        protected void OnEntityAdded(Entity entity)
        {
            EntityAdded?.Invoke(this, entity);
        }

        protected void OnEntityRemoved(Entity entity)
        {
            EntityRemoved?.Invoke(this, entity);
        }

        protected void OnEntityMoved(Entity entity)
        {
            EntityMoved?.Invoke(entity, (entity.X, entity.Y));
        }

        protected void OnEntityColorChanged(Entity entity)
        {
            EntityColorChanged?.Invoke(entity, entity.Color);
        }

        protected void OnEntityViewDistanceChanged(Entity entity)
        {
            EntityViewDistanceChanged?.Invoke(entity, entity.ViewDistance);
        }

        #endregion
        
        
    }

    public class EntityOptions
    {
        public static EntityOptions Default => new EntityOptions();
        
        public int? X;
        public int? Y;
        public int Color = 0xFFFFFF;
        public int ViewDistance = 0;
        public bool IsBlocking = true;
    }
    
    public class EntityInitialization
    {
        public readonly int Id;
        public int X;
        public int Y;
        public int Color = 0xFFFFFF;
        public int ViewDistance;
        public bool IsBlocking = true;

        public EntityInitialization(int id)
        {
            Id = id;
        }
    }
#pragma warning restore 1591
}