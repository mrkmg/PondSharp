using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    public class Clustering : BaseEntity
    {
        private int _thinkCooldown;
        private bool _isFleeing;
        private (int X, int Y) _lastForce = (0, 0);
        
        public override void OnCreated()
        {
            ChooseRandomDirection();
        }
        
        public override void Tick()
        {
            SetDirection();
            
            if (MoveTo(X + _forceX, Y + _forceY)) return;
            
            // reverse if stuck
            _forceX = -_forceX;
            _forceY = -_forceY;
            _thinkCooldown = 10;
        }

        private void SetDirection()
        {
            if (_thinkCooldown > 1)
            {
                _thinkCooldown--;
                return;
            }

            var entities = VisibleEntities.ToList();
            if (CheckFlee(entities)) return;
                
            ChooseForceDirections(entities);

            if (_random.Next(100) != 0) return;
            
            ChooseRandomDirection();
            _thinkCooldown = _random.Next(100);
        }

        private bool CheckFlee(IList<IAbstractEntity> entities)
        {
            if (!entities.Any()) return false;
            
            var closestEntity = entities
                .OrderBy(e => EntityDist(this, e))
                .First();
            var closestDistance = EntityDist(this, closestEntity);

            if (closestDistance > 3) return false;
            _lastForce = (_forceX, _forceY);
            (_forceX, _forceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (_forceX == 0 && _forceY == 0) ChooseRandomDirection();
            _isFleeing = true;
            _thinkCooldown = _random.Next(5, 20);;
            return true;
        }

        private void ChooseForceDirections(IList<IAbstractEntity> entities)
        {
            if (entities.Count == 0) return;
            
            var (tx, ty) = entities
                .Aggregate<IAbstractEntity, (int X, int Y)>((0, 0), (a, e) => (a.X + e.X, a.Y + e.Y));
            var (groupCenterX, groupCenterY) = (tx / entities.Count, ty / entities.Count);
            
            if (entities.Count > 10)
            {
                // Move away from group center
                (_forceX, _forceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                _thinkCooldown = entities.Count > 20 ? 20 : 10;
            } else if (Dist(X, Y, groupCenterX, groupCenterY) > 5)
            {
                // Move toward group center
                (_forceX, _forceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                _thinkCooldown = 10;
            }
        }
    }
}