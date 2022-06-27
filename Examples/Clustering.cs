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
    [PondDefaults(NewCount = 100)]
    public class Clustering : BaseEntity
    {
        private const int WanderingColor = 0xFFFFFF;  // White
        private const int JoiningColor = 0x5555FF;    // Blue
        private const int SeparatingColor = 0xFFFF55; // Yellow
        private const int RestingColor = 0x55FF55;    // Green

        [PondAdjustable(Min = 2, Max = 30, Name = "Group Radius")]
        private static int GroupSize { get; set; } = 5;
        
        [PondAdjustable(Min = 2, Max = 50, Name = "Group Count")]
        private static int GroupCount { get; set; } = 20;

        private int _thinkCooldown;

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
            
            // if stuck
            ChooseRandomDirection();
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
            //if (CheckFlee(entities)) return;
                
            ChooseForceDirections(entities);

            if (Random.Next(100) != 0) return;
            
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
            _thinkCooldown = RndThinkDelay(200, 2, 100);
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
            
            if (entities.Count > GroupCount)
            {
                ChangeColor(SeparatingColor);
                // Move away from group center
                (ForceX, ForceY) = GetForceDirection(X - groupCenterX, Y - groupCenterY);
                _thinkCooldown = RndThinkDelay(100, 1);
            } else if (distanceToCenter > GroupSize)
            {
                ChangeColor(JoiningColor);
                // Move toward group center
                (ForceX, ForceY) = GetForceDirection(groupCenterX - X, groupCenterY - Y);
                _thinkCooldown = RndThinkDelay(10);
            }
            else
            {
                ChangeColor(RestingColor);
                (ForceX, ForceY) = (0, 0);
                _thinkCooldown = RndThinkDelay(30);
            }
        }
    }
}