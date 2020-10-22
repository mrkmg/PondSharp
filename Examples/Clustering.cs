using System;
using System.Collections.Generic;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// This is a pretty neat little entity
    /// which will seek out other entities
    /// and form groups, unless the area is
    /// to dense, or it's too close to
    /// another entity.
    /// </summary>
    public class Clustering : BaseEntity
    {
        private const int WanderingColor = 0xFFFFFF;  // White
        private const int JoiningColor = 0x5555FF;    // Blue
        private const int SeparatingColor = 0xFFFF55; // Yellow
        private const int FleeingColor = 0xFF5555;    // Red
        private const int RestingColor = 0x55FF55;    // Green

        private int ThinkCooldown;

        private int RndThinkDelay(int max, int curve = 4, int divisors = 20) =>
            (int) (Math.Pow(_random.Next(divisors) + 1, curve) / Math.Pow(divisors, curve) * max); 
        
        public override void OnCreated()
        {
            ChangeViewDistance(30);
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
        }
        
        public override void Tick()
        {
            SetDirection();
            
            if (MoveTo(X + _forceX, Y + _forceY)) return;
            
            // reverse if stuck
            _forceX = -_forceX;
            _forceY = -_forceY;
            ThinkCooldown = 10;
        }

        private void SetDirection()
        {
            if (ThinkCooldown > 1)
            {
                ThinkCooldown--;
                return;
            }

            var entities = VisibleEntities.ToList();
            if (CheckFlee(entities)) return;
                
            ChooseForceDirections(entities);

            if (_random.Next(100) != 0) return;
            
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
            ThinkCooldown = RndThinkDelay(200, 5, 100);
        }

        private bool CheckFlee(IList<IEntity> entities)
        {
            if (!entities.Any(e => EntityDist(this, e) < 3)) 
                return false;
            
            var closestEntity = entities
                .OrderBy(e => EntityDist(this, e))
                .First();
            (_forceX, _forceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (_forceX == 0 && _forceY == 0) ChooseRandomDirection();
            ThinkCooldown = RndThinkDelay(30, 6);
            ChangeColor(FleeingColor);
            return true;
        }

        private void ChooseForceDirections(IList<IEntity> entities)
        {
            if (entities.Count == 0) 
            {
                ChangeColor(WanderingColor);
                return;
            }
            
            var (tx, ty) = entities
                .Aggregate<IEntity, (int X, int Y)>((0, 0), (a, e) => (a.X + e.X, a.Y + e.Y));
            var (groupCenterX, groupCenterY) = (tx / entities.Count, ty / entities.Count);
            var distanceToCenter = Dist(X, Y, groupCenterX, groupCenterY);
            
            if (entities.Count > 15)
            {
                ChangeColor(SeparatingColor);
                // Move away from group center
                (_forceX, _forceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                ThinkCooldown = RndThinkDelay(entities.Count);
            } else if (distanceToCenter > 5)
            {
                ChangeColor(JoiningColor);
                // Move toward group center
                (_forceX, _forceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                ThinkCooldown = RndThinkDelay(10);
            }
            else
            {
                ChangeColor(RestingColor);
                (_forceX, _forceY) = (0, 0);
            }
        }
    }
}