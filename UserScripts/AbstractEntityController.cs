using System;

namespace PondSharp.UserScripts
{
    public abstract class AbstractEntityController
    {
        public event EventHandler<(int, int)> Moved;

        protected virtual bool CanMoveTo(AbstractTestEntity entity, int x, int y) => true;
        
        public bool MoveTo(AbstractTestEntity entity, int x, int y)
        {
            if (!CanMoveTo(entity, x, y))
                return false;
            entity.X = x;
            entity.Y = y;
            Moved?.Invoke(entity, (x, y));
            return true;
        }
    }
}