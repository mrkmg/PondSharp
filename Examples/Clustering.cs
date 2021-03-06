using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [PondDefaults(NewCount = 100)]
    public class Clustering : BaseEntity
    {
        private const int WanderingColor = 0xFFFFFF;  // White
        private const int JoiningColor = 0x5555FF;    // Blue
        private const int SeparatingColor = 0xFFFF55; // Yellow
        private const int FleeingColor = 0xFF5555;    // Red
        private const int RestingColor = 0x55FF55;    // Green

        private int ThinkCooldown;

        private int RndThinkDelay(int max, int curve = 4, int divisors = 20) =>
            (int) (Math.Pow(Random.Next(divisors) + 1, curve) / Math.Pow(divisors, curve) * max); 
        
        protected override void OnCreated()
        {
            ChangeViewDistance(15);
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
        }
        
        protected override void Tick()
        {
            SetDirection();
            
            if (MoveTo(X + ForceX, Y + ForceY)) return;
            
            // reverse if stuck
            ForceX = -ForceX;
            ForceY = -ForceY;
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
            //if (CheckFlee(entities)) return;
                
            ChooseForceDirections(entities);

            if (Random.Next(100) != 0) return;
            
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
            ThinkCooldown = RndThinkDelay(200, 2, 100);
        }

        private bool CheckFlee(IList<IEntity> entities)
        {
            if (!entities.Any(e => EntityDist(this, e) < 3)) 
                return false;
            
            var closestEntity = entities
                .OrderBy(e => EntityDist(this, e))
                .First();
            (ForceX, ForceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (ForceX == 0 && ForceY == 0) ChooseRandomDirection();
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

            var total = entities.Count;
            var (groupCenterX, groupCenterY) = entities
                .Aggregate<IEntity, (int X, int Y)>((0, 0), (a, e) => (a.X + e.X/total, a.Y + e.Y/total));
            var distanceToCenter = Dist(X, Y, groupCenterX, groupCenterY);
            
            if (entities.Count > 5)
            {
                ChangeColor(SeparatingColor);
                // Move away from group center
                (ForceX, ForceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                ThinkCooldown = RndThinkDelay(100, 1);
            } else if (distanceToCenter > 5)
            {
                ChangeColor(JoiningColor);
                // Move toward group center
                (ForceX, ForceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                ThinkCooldown = RndThinkDelay(10);
            }
            else
            {
                ChangeColor(RestingColor);
                (ForceX, ForceY) = (0, 0);
                ThinkCooldown = RndThinkDelay(10);
            }
        }
    }
}