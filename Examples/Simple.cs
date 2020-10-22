using System;

namespace PondSharp.Examples
{
    /// <summary>
    /// The simplest example of an entity.
    /// </summary>
    public class Simple : BaseEntity
    {
        public override void OnCreated()
        {
            if (_random.Next(2) == 0)
                _forceX = _random.Next(2) == 0 ? 1 : -1;
            else
                _forceY = _random.Next(2) == 0 ? 1 : -1;
        }
        
        public override void Tick()
        {
            if (!MoveTo(X + _forceX, Y + _forceY))
            {
                _forceX = -_forceX;
                _forceY = -_forceY;
            }
        }
    }
}