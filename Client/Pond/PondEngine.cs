using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public sealed class PondEngine : Engine
    {
        private const int BlockSize = 30;

        public PondEngine(int minX, int maxX, int minY, int maxY)
        {
            ResetSize(minX, maxX, minY, maxY);
        }

        private void ResetSize(int minX, int maxX, int minY, int maxY)
        {
            _entitiesByBlock.Clear();
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            for (var x = minX; x < maxX + BlockSize; x += BlockSize)
            for (var y = minY; y < maxY + BlockSize; y += BlockSize)
                _entitiesByBlock[GetBlock(x, y)] = new HashSet<IEntity>();
        }


        private readonly Dictionary<int, HashSet<IEntity>> _entitiesByBlock = new Dictionary<int, HashSet<IEntity>>();
        private readonly Dictionary<int, IEntity> _entitiesById = new Dictionary<int, IEntity>();
        
        public override IEnumerable<IEntity> Entities => _entitiesById.Values;
        public override IEntity GetEntity(int entityId) => _entitiesById[entityId];
        
        private static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        private int GetBlock(IEntity e) => GetBlock(e.X, e.Y);
        private int GetBlock(int x, int y) => 
            (x - MinX) / BlockSize + (y - MinY) / BlockSize * (MaxX - MinX) / BlockSize;


        private IEnumerable<IEntity> GetEntitiesAround(int centerX, int centerY, int dist)
        {
            for (var x = centerX - dist; x < centerX + dist + BlockSize/2; x += BlockSize)
            for (var y = centerY - dist; y < centerY + dist + BlockSize/2; y += BlockSize)
                if (x >= MinX && x < MaxX && y >= MinY && y < MaxX)
                    foreach (var entity in _entitiesByBlock[GetBlock(x, y)].Where(entity => Dist(entity.X, entity.Y, centerX, centerY) <= dist))
                        yield return entity;
        }
        
        public override bool CanMoveTo(IEntity entity, int x, int y)
        {
            return Math.Abs(entity.X - x + entity.Y - y) <= 2 &&
                   x >= MinX && x <= MaxX &&
                   y >= MinY && y <= MaxY;
        }

        public void InsertEntity(IEntity entity)
        {
            _entitiesById.Add(entity.Id, entity);
            _entitiesByBlock[GetBlock(entity)].Add(entity);
            OnEntityAdded(entity);
        }
        
        public override bool MoveTo(IEntity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y)) return false;
            if (entity.X == x && entity.Y == y) return true;
            
            var oldBlock = GetBlock(entity);
            var newBlock = GetBlock(x, y);
            if (oldBlock != newBlock)
            {
                _entitiesByBlock[oldBlock].Remove(entity);
                _entitiesByBlock[newBlock].Add(entity);
            }
            WriteEntityPosition(entity, x, y);
            return true;
        }

        public override IEnumerable<IEntity> GetVisibleEntities(IEntity entity) =>
            GetEntitiesAround(entity.X, entity.Y, entity.ViewDistance).Where(e => e.Id != entity.Id);
        
        public bool RemoveEntity(int entityId)
        {
            var entity = _entitiesById[entityId];
            entity.OnDestroy();
            _entitiesByBlock[GetBlock(entity)].Remove(entity);
            return _entitiesById.Remove(entityId);
        }

        public void ClearAllEntities()
        {
            foreach (var entity in _entitiesById.Values)
            {
                entity.OnDestroy();
            }
            _entitiesById.Clear();
            foreach (var hashedEntities in _entitiesByBlock.Values)
                hashedEntities.Clear();
        }
    }
}