using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public sealed class PondEngine : Engine
    {
        private const int BlockSize = 30;

        public PondEngine(int width, int height)
        {
            _blockWidth = (int) Math.Ceiling((double)width / BlockSize);
            ResetSize(-width / 2, width / 2, -height / 2 - 1, height / 2 - 1);
        }
        
        private readonly int _blockWidth;
        private LinkedList<IEntity>[] _entitiesByBlock = { };
        private readonly Dictionary<int, IEntity> _entitiesById = new Dictionary<int, IEntity>();
        
        public override IEnumerable<IEntity> Entities => _entitiesById.Values;
        public override IEntity GetEntity(int entityId) => _entitiesById[entityId];
        
        private static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        private int GetBlock(IEntity e) => GetBlock(e.X, e.Y);
        private int GetBlock(int x, int y) => (x - MinX) / BlockSize + (y - MinY) / BlockSize * (_blockWidth);
        
        private void ResetSize(int minX, int maxX, int minY, int maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            _entitiesByBlock = new LinkedList<IEntity>[GetBlock(MaxX, MaxY) + 1];
            for (var i = 0; i < _entitiesByBlock.Length; i++) 
                _entitiesByBlock[i] = new LinkedList<IEntity>();
            foreach (var entity in _entitiesById.Values)
                _entitiesByBlock[GetBlock(entity)].AddLast(entity);
        }

        private IEnumerable<IEntity> GetEntitiesAround(int centerX, int centerY, int dist)
        {
            for (var x = centerX - dist; x < centerX + dist + BlockSize; x += BlockSize)
            for (var y = centerY - dist; y < centerY + dist + BlockSize; y += BlockSize)
                if (x >= MinX && x < MaxX && y >= MinY && y < MaxY)
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
            _entitiesByBlock[GetBlock(entity)].AddLast(entity);
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
                _entitiesByBlock[newBlock].AddLast(entity);
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
            foreach (var entities in _entitiesByBlock)
                entities.Clear();
        }
    }
}