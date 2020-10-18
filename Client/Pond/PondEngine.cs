using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public class PondEngine : AbstractEngine
    {
        private int _minX;
        private int _maxX;
        private int _minY;
        private int _maxY;

        public PondEngine(int minX, int maxX, int minY, int maxY)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
        }

        private Dictionary<string, AbstractEntity> _entities = new Dictionary<string, AbstractEntity>();
        public override IEnumerable<AbstractEntity> Entities { get => _entities.Values; }

        public override AbstractEntity GetEntity(string entityId) => _entities[entityId];

        protected bool CanMoveTo(AbstractEntity entity, int x, int y)
        {
            return Math.Abs(entity.X - x + entity.Y - y) <= 2 &&
                   x >= _minX && x <= _maxX &&
                   y >= _minY && y <= _maxY;
        }

        public void InsertEntity(AbstractEntity entity)
        {
            _entities.Add(entity.Id, entity);
            OnEntityAdded(entity);
        }
        
        public override bool MoveTo(AbstractEntity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y)) return false;
            if (entity.X != x || entity.Y != y)
            {
                WriteEntityPosition(entity, x, y);
                OnEntityMoved(entity);
            }
            return true;
        }

        public override bool SetColorTo(AbstractEntity entity, int color)
        {
            WriteEntityColor(entity, color);
            OnEntityColorChanged(entity);
            return true;
        }

        public override IEnumerable<AbstractEntity> GetVisibleEntities(AbstractEntity entity) => 
            _entities.Values
                .Where(e => EntityDist(entity, e) < entity.ViewDistance)
                .Where(e => e.Id != entity.Id);

        private static int EntityDist(AbstractEntity e1, AbstractEntity e2) => 
            (int) Math.Sqrt(Math.Pow(e1.X - e2.X, 2) + Math.Pow(e1.Y - e2.Y, 2));

        public bool RemoveEntity(string entityId) => _entities.Remove(entityId);
        public void ClearAllEntities() => _entities.Clear();
    }
}