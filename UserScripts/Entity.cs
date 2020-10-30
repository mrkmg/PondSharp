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
        
        /// <summary>
        /// Target position X to move toward.
        /// </summary>
        protected int TargetX { get; private set; }
        
        /// <summary>
        /// Target position Y to move toward.
        /// </summary>
        protected int TargetY { get; private set; }
        
        private double ApproxX { get; set; }
        private double ApproxY { get; set; }

        private bool _intialized;
        
        /// <summary>
        /// Did the entity move this tick;
        /// </summary>
        public bool DidMoveThisTick { get; internal set; }

        internal void Initialize(Engine engine, EntityInitialization initialization)
        {
            if (_intialized) throw new AlreadyInitializedException();
            Engine = engine;
            
            Id = initialization.Id;
            X = initialization.X;
            Y = initialization.Y;
            ApproxX = X;
            ApproxY = Y;
            Color = initialization.Color;
            ViewDistance = initialization.ViewDistance;
            
            _intialized = true;
        }

        /// <summary>
        /// Set a target position for the entity
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void SetMoveTowards(int x, int y)
        {
            TargetX = x;
            TargetY = y;
            ApproxX = X;
            ApproxY = Y;
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
        protected bool MoveTo(int x, int y)
        {
            if (DidMoveThisTick) return false;
            DidMoveThisTick = true;
            ApproxX = x;
            ApproxY = y;
            return Engine.MoveTo(this, x, y);
        }

        /// <summary>
        /// Execute a move towards the current set target position
        /// </summary>
        /// <returns></returns>
        protected bool MoveTowardsTarget()
        {
            if (DidMoveThisTick) return false;
            DidMoveThisTick = true;
            var (x, y) = (TargetX - ApproxX, TargetY - ApproxY);
            var dM = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            var (fx, fy) = dM == 0 ? (0, 0) : (x / dM, y / dM);
            ApproxX += fx;
            ApproxY += fy;
            return Engine.MoveTo(this, (int)Math.Round(ApproxX), (int)Math.Round(ApproxY));
        }

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