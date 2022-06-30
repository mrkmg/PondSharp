using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public sealed class PondEngine : Engine
    {
        public PondEngine(int width, int height)
        {
            MinX = -width / 2;
            MaxX = width / 2 - 1;
            MinY = -height / 2;
            MaxY = height / 2 - 1;
            ResetSize();
        }

        private int _nextId;
        private Layer<Entity> _entityLayer;
        private readonly List<Entity> _entities = new();
        private readonly Dictionary<int, Entity> _entitiesById = new();
        private readonly List<Entity> _entitiesToRemove = new();
        private readonly List<Entity> _entitiesToAdd = new();
        private readonly Random _random = new();
        
        public override IEnumerable<Entity> Entities => _entities;
        public override Entity GetEntity(int entityId) => _entitiesById[entityId];
        public override Entity GetEntityAt(int x, int y) => _entityLayer.GetAt(x, y);
        public int TotalEntities => _entities.Count;
        
        private static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        
        private void ResetSize()
        {
            _entityLayer = new(MinX, MinY, MaxX, MaxY);
            foreach (var entity in _entities)
            {
                _entityLayer.Add(entity, entity.X, entity.Y);
            }
        }

        private IEnumerable<Entity> GetEntitiesAround(int centerX, int centerY, int dist)
        {
            return _entityLayer.GetNear(centerX, centerY, dist).Where(entity => Dist(entity.X, entity.Y, centerX, centerY) <= dist);
        }

        public void Tick()
        {
            foreach (var entity in _entitiesToRemove)
                DestroyEntityActual(entity);
            _entitiesToRemove.Clear();
            
            foreach (var entity in _entitiesToAdd)
                InsertEntityActual(entity);
            _entitiesToAdd.Clear();
            
            foreach (var entity in _entities)
                DoTick(entity);
        }
        
        public override bool CanMoveTo(Entity entity, int x, int y)
            => Math.Abs(entity.X - x + entity.Y - y) <= 2 &&
               x >= MinX && x <= MaxX &&
               y >= MinY && y <= MaxY && 
               !(_entityLayer.GetAt(x, y)?.IsBlocking ?? false);

        public override bool CanChangeIsBlocking(Entity entity) => true;
        public override bool CanCreateEntity<T>(Entity entity) => true;
        
        public override T CreateEntity<T>(EntityOptions options)
        {
            var entity = Activator.CreateInstance<T>();
            InsertEntity(entity, options);
            return entity;
        }

        private readonly object _idLock = new();
        public void InsertEntity(Entity entity, EntityOptions options)
        {
            int id;
            lock (_idLock)
                id = _nextId++;
            InitializeEntity(entity, new (id)
            {
                X = options.X ?? _random.Next(MinX, MaxX),
                Y = options.Y ?? _random.Next(MinY, MaxY),
                Color = options.Color,
                ViewDistance = options.ViewDistance,
                IsBlocking = options.IsBlocking
            });
            _entitiesToAdd.Add(entity);
        }

        private void InsertEntityActual(Entity entity)
        {
            try
            {
                _entitiesById.Add(entity.Id, entity);
                _entityLayer.Add(entity, entity.X, entity.Y);
                _entities.Add(entity);
                OnEntityAdded(entity);
                WasCreated(entity);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error inserting entity ${entity.Id}");
                throw;
            }
        }
        
        public override bool MoveTo(Entity iEntity, int x, int y)
        {
            if (!(iEntity is Entity entity)) return false;
            if (entity.X == x && entity.Y == y) return true;
            if (!CanMoveTo(entity, x, y)) return false;

            _entityLayer.Move(entity, entity.X, entity.Y, x, y);
            WriteEntityPosition(entity, x, y);
            return true;
        }

        public override IEnumerable<Entity> GetVisibleEntities(Entity entity) =>
            GetEntitiesAround(entity.X, entity.Y, entity.ViewDistance).Where(e => e.Id != entity.Id);

        public override bool DestroyEntity(Entity entity)
        {
            if (!_entitiesById.ContainsKey(entity.Id)) return false;
            // This is inefficient if many entities are removed every tick.
            if (_entitiesToRemove.Contains(entity)) return false; 
            _entitiesToRemove.Add(entity);
            return true;
        }

        private void DestroyEntityActual(Entity entity)
        {
            WasDestroyed(entity);
            OnEntityRemoved(entity);
            _entityLayer.Remove(entity, entity.X, entity.Y);
            _entities.Remove(entity);
            _entitiesById.Remove(entity.Id);
        }

        public void ClearAllEntities()
        {
            _entities.Clear();
            foreach (var entity in _entitiesById.Values)
            {
                try
                {
                    WasDestroyed(entity);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message} {e.StackTrace}");
                }
            }
            _entitiesById.Clear();
            _entityLayer = new(MinX, MinY, MaxX, MaxY);
        }
    }
}