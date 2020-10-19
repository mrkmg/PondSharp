using System;
using System.Collections.Generic;

namespace PondSharp.UserScripts
{
    public abstract class AbstractEntity : IAbstractEntity
    {
        public string Id { get; private set; }
        private IEngine Engine { get; set; }
        
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public int Color { get; internal set; }
        public int ViewDistance { get; internal set; }

        private bool _intialized;

        public virtual void Initialize(string id, IEngine engine, int x = 0, int y = 0, int color = 0xFFFFFF, int viewDistance = 0)
        {
            if (_intialized) throw new AlreadyInitializedException();
            Id = id;
            Engine = engine;
            X = x;
            Y = y;
            Color = color;
            _intialized = true;
            ViewDistance = viewDistance;
            
            OnCreated();
        }

        public bool CanMoveTo(int x, int y) => Engine.CanMoveTo(this, x, y);
        public bool MoveTo(int x, int y) => Engine.MoveTo(this, x, y);

        public bool CanChangeColorTo(int color) => Engine.CanChangeColorTo(this, color);
        public bool ChangeColor(int color) => Engine.ChangeColorTo(this, color);
        public IEnumerable<IAbstractEntity> VisibleEntities => Engine.GetVisibleEntities(this);

        public virtual void OnCreated() {}
        public abstract void Tick();

        public class AlreadyInitializedException : Exception
        {
        }
    }
}