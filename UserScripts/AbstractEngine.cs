using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    public abstract class AbstractEngine
    {
        public abstract IEnumerable<AbstractEntity> Entities { get; }
        public abstract AbstractEntity GetEntity(string entityId);
        
        public bool MoveTo(string entityId, int x, int y) => MoveTo(GetEntity(entityId), x, y);
        public abstract bool MoveTo(AbstractEntity entity, int x, int y);
        protected void WriteEntityPosition(AbstractEntity entity, int x, int y)
        {
            entity.X = x;
            entity.Y = y;
        }
        
        public bool SetColorTo(string entityId, int color) => SetColorTo(GetEntity(entityId), color);
        public abstract bool SetColorTo(AbstractEntity entity, int color);
        protected void WriteEntityColor(AbstractEntity entity, int color)
        {
            entity.Color = color;
        }
        
        public IEnumerable<AbstractEntity> GetVisibleEntities(string entityId) => GetVisibleEntities(GetEntity(entityId));
        public abstract IEnumerable<AbstractEntity> GetVisibleEntities(AbstractEntity entity);
        
        
        public event EventHandler<(int, int)> EntityMoved;
        public event EventHandler<AbstractEntity> EntityAdded;
        public event EventHandler<int> EntityColorChanged;
        
        protected void OnEntityAdded(AbstractEntity entity)
        {
            EntityAdded?.Invoke(this, entity);
        }

        protected void OnEntityMoved(AbstractEntity entity)
        {
            EntityMoved?.Invoke(entity, (entity.X, entity.Y));
        }

        protected void OnEntityColorChanged(AbstractEntity entity)
        {
            EntityColorChanged?.Invoke(entity, entity.Color);
        }
    }
}