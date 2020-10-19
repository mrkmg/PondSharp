using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    public abstract class AbstractEngine : IEngine
    {
        public abstract IEnumerable<IAbstractEntity> Entities { get; }
        public abstract IAbstractEntity GetEntity(string entityId);
        public IEnumerable<IAbstractEntity> GetVisibleEntities(string entityId) => GetVisibleEntities(GetEntity(entityId));
        public abstract IEnumerable<IAbstractEntity> GetVisibleEntities(IAbstractEntity entity);

        #region Movement
        
        public virtual bool CanMoveTo(IAbstractEntity entity, int x, int y) => true;
        public bool MoveTo(string entityId, int x, int y) => MoveTo(GetEntity(entityId), x, y);
        public virtual bool MoveTo(IAbstractEntity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y)) return false;
            WriteEntityPosition(entity, x, y);
            return true;
        }
        protected void WriteEntityPosition(IAbstractEntity entity, int x, int y)
        {
            if (!(entity is AbstractEntity aEntity)) return;
            aEntity.X = x;
            aEntity.Y = y;
        }
        
        #endregion

        #region Color

        public virtual bool CanChangeColorTo(IAbstractEntity entity, int color) => true;
        public bool ChangeColorTo(string entityId, int color) => ChangeColorTo(GetEntity(entityId), color);
        public virtual bool ChangeColorTo(IAbstractEntity entity, int color)
        {
            if (!CanChangeColorTo(entity, color)) return false;
            WriteEntityColor(entity, color);
            return true;
        }
        protected void WriteEntityColor(IAbstractEntity entity, int color)
        {
            if (!(entity is AbstractEntity aEntity)) return;
            aEntity.Color = color;
        }

        #endregion
        
        

        #region Events

        public event EventHandler<(int, int)> EntityMoved;
        public event EventHandler<IAbstractEntity> EntityAdded;
        public event EventHandler<int> EntityColorChanged;
        
        protected void OnEntityAdded(IAbstractEntity entity)
        {
            EntityAdded?.Invoke(this, entity);
        }

        protected void OnEntityMoved(IAbstractEntity entity)
        {
            EntityMoved?.Invoke(entity, (entity.X, entity.Y));
        }

        protected void OnEntityColorChanged(IAbstractEntity entity)
        {
            EntityColorChanged?.Invoke(entity, entity.Color);
        }

        #endregion
        
        
    }
}