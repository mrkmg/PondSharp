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
        private const int WanderingColor = 0xAAAA00;
        private const int JoiningColor = 0x00AA00; 
        private const int SeparatingColor = 0xAA0000;

        [PondAdjustable(Min = 2, Max = 30, Name = "Group Radius")]
        private static int GroupRadius { get; set; } = 3;
        
        [PondAdjustable(Min = 2, Max = 30, Name = "View Radius")]
        private static int ViewRadius { get; set; } = 15;
        
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

            if (Random.Next(200) == 0)
                DoWander();
            else
                SetDirection();


            if (_hasTarget)
            {
                MoveTowardsTarget();

                if (TargetX == X && TargetY == Y)
                {
                    _hasTarget = false;
                }
            }
            
            if((ForceX == 0 && ForceY == 0) || MoveTo(X + ForceX, Y + ForceY))
                return;
            
            ChooseRandomDirection();
        }

        private void SetDirection()
        {
            if (--_thinkCooldown > 0)
                return;
            
            GroupColor = null;
            _hasTarget = false;
            ForceX = 0;
            ForceY = 0;
            
            var totalInView = 0;
            var totalInGroup = 0;
            var inViewCenterX = 0;
            var inViewCenterY = 0;
            var groupCenterX = 0;
            var groupCenterY = 0;
            int? groupColor = null;
            
            foreach (var entity in VisibleEntities)
            {
                if (!(entity is Clustering e)) continue;
                
                totalInView++;
                inViewCenterX += e.X;
                inViewCenterY += e.Y;
                
                if (EntityDist(this, e) >= GroupRadius) continue;
                
                totalInGroup++;
                groupCenterX += e.X;
                groupCenterY += e.Y;
                
                if (groupColor == null && e.GroupColor != null)
                    groupColor = e.GroupColor;
            }
            
            if (totalInView == 0)
            {
                DoWander();
                return;
            }
            
            inViewCenterX /= totalInView;
            inViewCenterY /= totalInView;
            if (totalInGroup > 0)
            {
                groupCenterX /= totalInGroup;
                groupCenterY /= totalInGroup;
            }
            var groupCenterDist = Dist(X, Y, groupCenterX, groupCenterY);

            if (totalInView > GroupCount)
            {
                DoLeaveGroup(inViewCenterX, inViewCenterY);
                return;
            }
            
            if (groupCenterDist <= GroupRadius)
            {
                DoGroupJoin(groupColor, inViewCenterX, inViewCenterY);
                return;
            }
            
            DoViewJoin(inViewCenterX, inViewCenterY);
        }

        private void DoLeaveGroup(int inViewCenterX, int inViewCenterY)
        {
            ChangeColor(SeparatingColor);
            // Move away from group center
            (ForceX, ForceY) = GetForceDirection(X - inViewCenterX, Y - inViewCenterY);
            _thinkCooldown = RndThinkDelay(100, 1);
        }

        private void DoGroupJoin(int? groupColor, int inViewCenterX, int inViewCenterY)
        {
            GroupColor = groupColor ?? RandomColor();
            ChangeColor(GroupColor.Value);
            (ForceX, ForceY) = (0, 0);
            _hasTarget = true;
            SetMoveTowards(inViewCenterX, inViewCenterY);
            _thinkCooldown = RndThinkDelay(100);
        }

        private void DoViewJoin(int inViewCenterX, int inViewCenterY)
        {
            ChangeColor(JoiningColor);
            // Move toward group center
            (ForceX, ForceY) = (0, 0);
            _hasTarget = true;
            SetMoveTowards(inViewCenterX, inViewCenterY);
            _thinkCooldown = RndThinkDelay(10);
        }

        private void DoWander()
        {
            _hasTarget = false;
            ChangeColor(WanderingColor);
            ChooseRandomDirection();
            Console.WriteLine($"WanderForce({ForceX},{ForceY})");
            if (ForceX == 0 && ForceY == 0) throw new Exception("INVALID FORCE");
            _thinkCooldown = RndThinkDelay(200, 4, 100);
        }

        private int RandomColor()
        {
            System.Drawing.Color color;
            do color = System.Drawing.Color.FromArgb(Random.Next(255), Random.Next(255), Random.Next(255));
            while (color.GetBrightness() < 0.65);
            return color.ToArgb();
        }
    }
}