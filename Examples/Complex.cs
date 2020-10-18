﻿using System.Linq;
using System.Threading.Tasks;

namespace PondSharp.Examples
{
    public class Complex : BaseEntity
    {
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

            if (_random.Next(100) == 0) 
            {
                ChooseRandomDirection();
                _thinkCooldown = _random.Next(20);
            }

            if (MoveTo(X + _forceX, Y + _forceY)) return;
            
            // reverse if stuck
            _forceX = -_forceX;
            _forceY = -_forceY;
            _thinkCooldown = 50;
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
            
            if (entities.Count > 10)
            {
                // Move away from group center
                (_forceX, _forceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                _thinkCooldown = entities.Count > 20 ? 50 : 5;
            } else if (Dist(X, Y, groupCenterX, groupCenterY) > 5)
            {
                // Move toward group center
                (_forceX, _forceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                _thinkCooldown = 10;
            }
            else
            {
                _thinkCooldown = 5;
            }

        }


    }
}