using System;
using PondSharp.UserScripts;

namespace PondSharp.Client
{
    public class TestEntityController : AbstractEntityController
    {
        private int _minX;
        private int _maxX;
        private int _minY;
        private int _maxY;

        public TestEntityController(int minX, int maxX, int minY, int maxY)
        {
            _minX = minX;
            _maxX = maxX;
            _minY = minY;
            _maxY = maxY;
        }

        protected override bool CanMoveTo(AbstractTestEntity entity, int x, int y)
        {
            return Math.Abs(entity.X - x + entity.Y - y) <= 2 &&
                   x >= _minX && x <= _maxX &&
                   y >= _minY && y <= _maxY;
        }
    }
}