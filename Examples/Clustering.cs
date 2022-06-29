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
    [PondDefaults(InitialCount = 2)]
    public class Clustering : BaseEntity
    {
        private const int WanderingColor = 0xFFFFFF;  // White
        private const int JoiningColor = 0x5555FF;    // Blue
        private const int SeparatingColor = 0xFFFF55; // Yellow

        [PondAdjustable(Min = 2, Max = 30, Name = "Group Radius")]
        private static int GroupRadius { get; set; } = 5;
        
        [PondAdjustable(Min = 2, Max = 30, Name = "View Radius")]
        private static int ViewRadius { get; set; } = 10;
        
        [PondAdjustable(Min = 2, Max = 50, Name = "Group Count")]
        private static int GroupCount { get; set; } = 9;

        public int? GroupColor { get; private set; }

        private int _thinkCooldown;
        private bool _hasTarget;

        private int RndThinkDelay(int max, int curve = 4, int divisors = 20) =>
            (int) (Math.Pow(Random.Next(divisors) + 1, curve) / Math.Pow(divisors, curve) * max); 
        
        protected override void OnCreated()
        {
            ChangeViewDistance(ViewRadius);
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
        }
        
        protected override void Tick()
        {
            if (ViewDistance != ViewRadius)
                ChangeViewDistance(ViewRadius);
            
            SetDirection();

            if (_hasTarget)
            {
                MoveTowardsTarget();
                return;
            }
            
            if (MoveTo(X + ForceX, Y + ForceY))
                return;
            
            // if stuck
            ChooseRandomDirection();
            _thinkCooldown = 10;
        }

        private void SetDirection()
        {


            var visibleEntities = VisibleEntities.ToList();
            var totalInView = visibleEntities.Count();
            
            if (totalInView == 0 || Random.Next(100) == 0) 
            {
                GroupColor = null;
                _hasTarget = false;
                ChangeColor(WanderingColor);
                ChooseRandomDirection();
                _thinkCooldown = RndThinkDelay(200, 4, 100);
                return;
            }
            
            if (--_thinkCooldown > 0)
                return;
            
            GroupColor = null;
            _hasTarget = false;
            
            var groupEntities = visibleEntities.Where(e => EntityDist(this, e) < GroupRadius).ToList();
            var totalInGroup = groupEntities.Count;

            var inViewCenterX = visibleEntities.Sum(e => e.X) / totalInView;
            var inViewCenterY = visibleEntities.Sum(e => e.Y) / totalInView;
            
            var groupCenterX = totalInGroup == 0 ? inViewCenterX : groupEntities.Sum(e => e.X) / totalInGroup;
            var groupCenterY = totalInGroup == 0 ? inViewCenterY : groupEntities.Sum(e => e.Y) / totalInGroup;
            var groupCenterDist = Dist(X, Y, groupCenterX, groupCenterY);

            if (totalInView > GroupCount)
            {
                ChangeColor(SeparatingColor);
                // Move away from group center
                // _hasTarget = true;
                (ForceX, ForceY) = GetForceDirection(X - inViewCenterX, Y - inViewCenterY);
                _thinkCooldown = RndThinkDelay(100, 1);
                return;
            }
            
            if (groupCenterDist <= GroupRadius)
            {
                var entity = (Clustering)groupEntities.FirstOrDefault(e => e is Clustering { GroupColor: {} });
                GroupColor = entity?.GroupColor ?? RandomColor();
                ChangeColor(GroupColor.Value);
                (ForceX, ForceY) = (0, 0);
                _hasTarget = true;
                SetMoveTowards(inViewCenterX, inViewCenterY);
                _thinkCooldown = RndThinkDelay(100);
                return;
            }
            
            ChangeColor(JoiningColor);
            // Move toward group center
            (ForceX, ForceY) = GetForceDirection(inViewCenterX - X, inViewCenterY - Y);
            _thinkCooldown = RndThinkDelay(10);
        }

        private int RandomColor()
        {
            System.Drawing.Color color;
            do color = System.Drawing.Color.FromArgb(Random.Next(255), Random.Next(255), Random.Next(255));
            while (color.GetBrightness() < 0.5);
            return color.ToArgb();
        }
    }
}