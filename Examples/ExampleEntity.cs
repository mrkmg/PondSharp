using System;
using System.Linq;
using System.Threading.Tasks;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    public class ExampleEntity : AbstractEntity
    {
        private readonly Random _random = new Random();

        private int _forceX = 0;
        private int _forceY = 0;
        private int _thinkCooldown = 0;

        public override void OnCreated()
        {
            ChooseRandomDirection();
        }

        public override void Tick()
        {
            if (_thinkCooldown <= 0)
                ChooseForceDirections();
            else
                _thinkCooldown--;

            if (_random.Next(50) == 0) 
            {
                ChooseRandomDirection();
                _thinkCooldown = 10;
            }
            MoveTo(X + _forceX, Y + _forceY);
        }

        private void ChooseForceDirections()
        {
            var entities = VisibleEntities.ToList();
            if (entities.Count == 0) return;
            
            var closestEntity = entities
                .OrderBy(e => EntityDist(this, e))
                .First();
            var closestDistance = EntityDist(this, closestEntity);
            if (closestDistance < 3)
            {
                (_forceX, _forceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
                if (_forceX == 0 && _forceY == 0) ChooseRandomDirection();
                _thinkCooldown = 3;
                return;
            }
            
            var (tx, ty) = entities.Aggregate((0, 0), (a, e) => (a.Item1 + e.X, a.Item2 + e.Y));
            var (groupCenterX, groupCenterY) = (tx / entities.Count, ty / entities.Count);
            
            if (entities.Count > 20)
            {
                // Move away from group center
                (_forceX, _forceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                _thinkCooldown = entities.Count > 30 ? 30 : 5;
            } else if (Dist(X, Y, groupCenterX, groupCenterY) > 5)
            {
                // Move toward group center
                (_forceX, _forceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
            }
            else
            {
                _thinkCooldown = 5;
            }

        }

        private void ChooseRandomDirection()
        {
            _forceX = _random.Next(-1, 2);
            _forceY = _random.Next(-1, 2);
        }

        private static (int, int) GetForceDirection(int x, int y)
        {
            var dM = Math.Max(Math.Abs(x), Math.Abs(y));
            return dM == 0 ? (0, 0) : ((int)Math.Round((double)x / dM), (int)Math.Round((double)y / dM));
        }
        
        private static int EntityDist(AbstractEntity e1, AbstractEntity e2) => 
            Dist(e1.X, e1.Y, e2.X, e2.Y);
        
        private static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));


    }
}