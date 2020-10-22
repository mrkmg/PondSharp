using System;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// A base entity with some helper classes for the
    /// examples.
    /// </summary>
    public abstract class BaseEntity : AbstractEntity
    {
        protected int _forceX = 0;
        protected int _forceY = 0;
        protected readonly Random _random = new Random();
        
        protected void ChooseRandomDirection()
        {
            _forceX = _random.Next(-1, 2);
            _forceY = _random.Next(-1, 2);
        }

        protected static (int, int) GetForceDirection(int x, int y)
        {
            var dM = Math.Max(Math.Abs(x), Math.Abs(y));
            return dM == 0 ? (0, 0) : ((int)Math.Round((double)x / dM), (int)Math.Round((double)y / dM));
        }
        
        protected static int EntityDist(IEntity e1, IEntity e2) => 
            Dist(e1.X, e1.Y, e2.X, e2.Y);
        
        protected static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }
}