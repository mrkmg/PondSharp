using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    /// <summary>
    /// Abstract entity class. All entities should extend this class to be properly loaded into the Pond.
    /// </summary>
    public abstract class Entity : IEntity
    {
        private int _id;
        /// <inheritdoc />
        public int Id
        {
            get => _id;
            private set
            {
                if (_intialized) throw new InvalidOperationException("Already Initialized");
                _id = value;
            }
        }
        
        private Engine Engine { get; set; }

        /// <inheritdoc />
        public int X { get; internal set; }

        /// <inheritdoc />
        public int Y { get; internal set; }

        /// <inheritdoc />
        public int Color { get; internal set; }

        /// <inheritdoc />
        public int ViewDistance { get; internal set; }

        /// <inheritdoc />
        public int WorldMinX => Engine.MinX;

        /// <inheritdoc />
        public int WorldMaxX => Engine.MaxX;

        /// <inheritdoc />
        public int WorldMinY => Engine.MinY;

        /// <inheritdoc />
        public int WorldMaxY => Engine.MaxY;

        private bool _intialized;

        /// <summary>
        /// Initialize Entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="engine"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="viewDistance"></param>
        /// <exception cref="AlreadyInitializedException"></exception>
        internal void Initialize(int id, Engine engine, int x = 0, int y = 0, int color = 0xFFFFFF, int viewDistance = 0)
        {
            if (_intialized) throw new AlreadyInitializedException();
            Id = id;
            Engine = engine;
            X = x;
            Y = y;
            Color = color;
            _intialized = true;
            ViewDistance = viewDistance;
        }

        /// <summary>
        /// Checks if the entity can move to a World Position
        /// </summary>
        /// <param name="x">World X Position</param>
        /// <param name="y">World Y Position</param>
        /// <returns>If the entity can move to the World Position</returns>
        protected bool CanMoveTo(int x, int y) => Engine.CanMoveTo(this, x, y);

        /// <summary>
        /// Tries to move the entity to the World Position
        /// </summary>
        /// <param name="x">World X Position</param>
        /// <param name="y">World Y Position</param>
        /// <returns>If the entity moved to the World Position</returns>
        protected bool MoveTo(int x, int y) => Engine.MoveTo(this, x, y);

        /// <summary>
        /// Checks if entity can change its color to the specified color.
        /// </summary>
        /// <param name="color">Desired Color</param>
        /// <returns>If the entity can change its color to the specified color.</returns>
        protected bool CanChangeColorTo(int color) => Engine.CanChangeColorTo(this, color);

        /// <summary>
        /// Tries to change the entity's color to the specified color.
        /// </summary>
        /// <param name="color">Desired Color</param>
        /// <returns>If the entity's color was changed to the specified color.</returns>
        protected bool ChangeColor(int color) => Engine.ChangeColorTo(this, color);

        /// <summary>
        /// Checks if entity can changes its view distance to the specified distance.
        /// </summary>
        /// <param name="distance">Distance that visible entities will use.</param>
        /// <returns>If the entity can change its view distance to the specified distance.</returns>
        /// 
        protected bool CanChangeViewDistance(int distance) => Engine.CanChangeViewDistance(this, distance);

        /// <summary>
        /// Tries to change the entity's view distance to the specified distance.
        /// </summary>
        /// <param name="distance">Distance that visible entities will use.</param>
        /// <returns>If the entity's view distance was change the specified distance.</returns>
        protected bool ChangeViewDistance(int distance) => Engine.ChangeViewDistance(this, distance);

        /// <summary>
        /// Entities which are within the ViewDistance
        /// </summary>
        /// <see cref="ViewDistance"/>
        protected IEnumerable<IEntity> VisibleEntities => Engine.GetVisibleEntities(this);

        /// <summary>
        /// Called immediately after the Entity is created and loaded into the world.
        ///
        /// Useful for initialization tasks.
        /// </summary>
        protected virtual void OnCreated(){}
        
        /// <summary>
        /// Called before the Entity is destroyed and removed from the world.
        /// </summary>
        protected virtual void OnDestroy(){}
        
        /// <summary>
        /// Called on every tick.
        /// </summary>
        protected virtual void Tick(){}

        internal void WasCreated() => OnCreated();
        internal void WasDestroyed() => OnDestroy();
        internal void DoTick() => Tick();
        
        
#pragma warning disable 1591
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool Equals(Entity other)
        {
            return Id == other.Id;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Entity) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
#pragma warning restore 1591

    /// <summary>
    /// Entity is already intialized.
    /// </summary>
    public class AlreadyInitializedException : InvalidOperationException
    {
    }
}