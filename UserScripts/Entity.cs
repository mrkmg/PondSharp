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
        
        private IEngine Engine { get; set; }

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

        /// <inheritdoc />
        public virtual void Initialize(int id, IEngine engine, int x = 0, int y = 0, int color = 0xFFFFFF, int viewDistance = 0)
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

        /// <inheritdoc />
        public bool CanMoveTo(int x, int y) => Engine.CanMoveTo(this, x, y);

        /// <inheritdoc />
        public bool MoveTo(int x, int y) => Engine.MoveTo(this, x, y);

        /// <inheritdoc />
        public bool CanChangeColorTo(int color) => Engine.CanChangeColorTo(this, color);

        /// <inheritdoc />
        public bool ChangeColor(int color) => Engine.ChangeColorTo(this, color);

        /// <inheritdoc />
        public bool CanChangeViewDistance(int distance) => Engine.CanChangeViewDistance(this, distance);

        /// <inheritdoc />
        public bool ChangeViewDistance(int distance) => Engine.ChangeViewDistance(this, distance);

        /// <inheritdoc />
        public IEnumerable<IEntity> VisibleEntities => Engine.GetVisibleEntities(this);

        /// <inheritdoc />
        public virtual void OnCreated() {}

        /// <inheritdoc />
        public virtual void OnDestroy() {}

        /// <inheritdoc />
        public abstract void Tick();
        
        
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
}