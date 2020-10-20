using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    public class Clustering : BaseEntity
    {
        private int _thinkCooldown;

        private int RndThinkDelay(int max, int curve = 4, int divisors = 20) =>
            (int) (Math.Pow(_random.Next(divisors) + 1, curve) / Math.Pow(divisors, curve) * max); 
        
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
            (_forceX, _forceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (_forceX == 0 && _forceY == 0) ChooseRandomDirection();
            _thinkCooldown = RndThinkDelay(50, 6);
            return true;
        }

        private void ChooseForceDirections(IList<IAbstractEntity> entities)
        {
            if (entities.Count == 0) return;
            
            var (tx, ty) = entities
                .Aggregate<IAbstractEntity, (int X, int Y)>((0, 0), (a, e) => (a.X + e.X, a.Y + e.Y));
            var (groupCenterX, groupCenterY) = (tx / entities.Count, ty / entities.Count);
            var distanceToCenter = Dist(X, Y, groupCenterX, groupCenterY);
            
            if (entities.Count > 10)
            {
                // Move away from group center
                (_forceX, _forceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                _thinkCooldown = RndThinkDelay(entities.Count);
            } else if (distanceToCenter > 2)
            {
                // Move toward group center
                (_forceX, _forceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                _thinkCooldown = RndThinkDelay(20);
            }
            else
            {
                (_forceX, _forceY) = (0, 0);
            }
        }
    }
}