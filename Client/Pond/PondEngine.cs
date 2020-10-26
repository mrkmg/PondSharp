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
            MinX = -width / 2;
            MaxX = width / 2;
            MinY = -height / 2 - 1;
            MaxY = height / 2 - 1;
            ResetSize();
        }
        
        private readonly int _blockWidth;
        private List<IEntity>[] _entitiesByBlock = { };
        private readonly Dictionary<int, IEntity> _entitiesById = new Dictionary<int, IEntity>();
        private List<IEntity>[][] _entitiesByPosition = { };
        private List<IEntity> _entities = new List<IEntity>();
        
        public override IEnumerable<IEntity> Entities => _entities;
        public override IEntity GetEntity(int entityId) => _entitiesById[entityId];
        
        private static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        private int GetBlock(IEntity e) => GetBlock(e.X, e.Y);
        private int GetBlock(int x, int y) => (x - MinX) / BlockSize + (y - MinY) / BlockSize * (_blockWidth);
        
        private void ResetSize()
        {
            _entitiesByBlock = new List<IEntity>[GetBlock(MaxX, MaxY) + 1];
            for (var i = 0; i < _entitiesByBlock.Length; i++) 
                _entitiesByBlock[i] = new List<IEntity>();
            
            _entitiesByPosition = new List<IEntity>[MaxX - MinX + 1][];
            for (var x = 0; x <= MaxX - MinX; x++)
            {
                _entitiesByPosition[x] = new List<IEntity>[MaxY - MinY + 1];
                for (var y = 0; y <= MaxY - MinY; y++)
                    _entitiesByPosition[x][y] = new List<IEntity>();
                
            }

            foreach (var entity in _entities)
            {
                _entitiesByBlock[GetBlock(entity)].Add(entity);
                _entitiesByPosition[entity.X - MinX][entity.Y - MinY].Add(entity);
            }
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
                   x >= MinX && x < MaxX &&
                   y >= MinY && y < MaxY && 
                   _entitiesByPosition[x - MinX][y - MinY].Count == 0;
        }

        public void InsertEntity(IEntity entity)
        {
            _entitiesById.Add(entity.Id, entity);
            _entitiesByBlock[GetBlock(entity)].Add(entity);
            _entitiesByPosition[entity.X - MinX][entity.Y - MinY].Add(entity);
            _entities.Add(entity);
            OnEntityAdded(entity);
        }
        
        public override bool MoveTo(IEntity entity, int x, int y)
        {
            if (entity.X == x && entity.Y == y) return true;
            if (!CanMoveTo(entity, x, y)) return false;
            
            _entitiesByPosition[entity.X - MinX][entity.Y - MinY].Remove(entity);
            _entitiesByPosition[x - MinX][y - MinY].Add(entity);
            
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
            _entitiesByPosition[entity.X - MinX][entity.Y - MinY].Remove(entity);
            _entities.Remove(entity);
            return _entitiesById.Remove(entityId);
        }

        public void ClearAllEntities()
        {
            _entities.Clear();
            foreach (var entity in _entitiesById.Values)
            {
                entity.OnDestroy();
            }
            _entitiesById.Clear();
            
            foreach (var entities in _entitiesByBlock)
                entities.Clear();
            
            foreach (var ex in _entitiesByPosition)
            foreach (var entities in ex)
                entities.Clear();
        }
    }
}