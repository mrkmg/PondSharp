using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// A Stated entity will try to join a group of other entities.
    ///
    /// The groups are stored in a static list.
    /// 
    /// When an entity is created, it first checks
    /// if there are any open groups, and if so
    /// then joins that group. If not, then it
    /// creates a new group and assigns itself as
    /// the leader of that group.
    ///
    /// The leader of the group will tick the whole
    /// group, checking what the current state is,
    /// and setting destinations for all the followers
    /// in that group. 
    /// </summary>
    [PondDefaults(InitialCount = 500, NewCount = 100)]
    public class Stated : Entity
    {
        private static readonly Random Random = new Random();
        private int DestinationX;
        private int DestinationY;
        private int StuckCounter;
        private State MyState;
        private int FleeX;
        private int FleeY;
        private int FleeCooldown;

        private static List<State> States = new List<State>();
        private static int TotalEntities => States.Sum(s => s.Followers.Count + 1);
        private static int TotalEntitiesArea => (int) Math.Sqrt(TotalEntities / Math.PI);

        protected override void OnCreated()
        {
            StuckCounter = 10;
            DestinationX = X;
            DestinationY = Y;

            MyState = States.FirstOrDefault(s => !s.IsFull);
            if (MyState == null)
            {
                MyState = new State {Leader = this};
                States.Add(MyState);
            }
            
            MyState.Followers.Add(this);

            foreach (var state in States)
            {
                state.CurrentState = CurrentStateType.Exploding;
            }
            ChangeColor(MyState.StateColor);
        }

        protected override void OnDestroy()
        {
            if (MyState == null) return;
            if (MyState.Leader?.Id == Id) MyState.Leader = null;
            MyState.Followers.Remove(this);
            if (MyState.Followers.Count == 0) States.Remove(MyState);
            MyState = null;
        }

        protected override void Tick()
        {
            if (MyState == null)
            {
                if (Color != 0xFF0000) 
                    ChangeColor(0xFF0000);
                return;
            }
            
            if (MyState.Leader.Id == Id) MyState.Tick();

            if (X == DestinationX && Y == DestinationY) return;

            if (FleeCooldown > 0)
            {
                FleeCooldown--;
                var t = Random.Next(9);
                if (t == 0) MoveTo(X - FleeX, Y - FleeY);
                else if (t < 5)  MoveTo(X + FleeY, Y - FleeX);
                else MoveTo(X - FleeY, Y + FleeX);
                return;
            }

            if (MoveTowardsTarget()) return;
            
            var (x, y) = ((double)DestinationX - X, (double)DestinationY - Y);
            var dM = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            var (fx, fy) = dM == 0 ? (0, 0) : ((int)Math.Round(x / dM), (int)Math.Round(y / dM));
            
            if (dM <= MyState.CurrentMaxStuckDistance && --StuckCounter <= 0)
            {
                DestinationX = X;
                DestinationY = Y;
                return;
            }

            FleeCooldown = 3;
            FleeX = fx;
            FleeY = fy;
        }

        private void SetMovePoint(int x, int y, int maxStuckCount)
        {
            DestinationX = x;
            DestinationY = y;
            StuckCounter = maxStuckCount;
            SetMoveTowards(x, y);
        }

        private class State
        {
            private static (int, int) RandomPointOnCircle(int x, int y, int dist, double angle)
            {
                return (x + (int)(dist * Math.Sin(angle)), y + (int)(dist * Math.Cos(angle)));
            }

            private static int RandomColor()
            {
                System.Drawing.Color color;
                do color = System.Drawing.Color.FromArgb(Random.Next(255), Random.Next(255), Random.Next(255));
                while (color.GetBrightness() < 0.5);
                return color.ToArgb();
            }

            public Stated Leader { get; set; }
            public List<Stated> Followers { get; } = new List<Stated>();
            public int StateColor = RandomColor();
            public bool IsFull => Followers.Count >= GroupSize;
            public CurrentStateType CurrentState { get; set; } = CurrentStateType.WaitingInCircle;
            public int CurrentCenterX;
            public int CurrentCenterY;
            public int CurrentDistance;
            public int GroupSize = 5 + Random.Next(20);
            public int CurrentMaxStuckDistance = int.MaxValue;
            public int CurrentTaskTimeout = 100;

            private bool DestinationsFulfilled =>
                !(Leader?.DestinationX != Leader?.X || Leader?.DestinationY != Leader?.Y ||
                  Followers.Any(f => f.X != f.DestinationX || f.Y != f.DestinationY));

            public void Tick()
            {
                CurrentTaskTimeout--;
                switch (CurrentState)
                {
                    case CurrentStateType.WaitingInCenter:
                    {
                        if (States.Any(s => s.CurrentState == CurrentStateType.MovingTowardCenter) && CurrentTaskTimeout > 0) break;
                        
                        StartMovingTowardExplosion();
                        break;
                    }
                    case CurrentStateType.MovingToExplosionPoint:
                        if (!DestinationsFulfilled && CurrentTaskTimeout > 0) break;
                        
                        StartExploding();
                        break;
                    case CurrentStateType.Exploding:
                        if (!DestinationsFulfilled && CurrentTaskTimeout > 0) break;
                        CurrentState = CurrentStateType.WaitingInCircle;
                        CurrentTaskTimeout = 3000;
                        
                        break;
                    case CurrentStateType.WaitingInCircle:
                        if (States.Any(s => s.CurrentState == CurrentStateType.Exploding || s.CurrentState == CurrentStateType.MovingToExplosionPoint) && CurrentTaskTimeout > 0) break;

                        StartMovingTowardCenter();
                        break;
                    case CurrentStateType.MovingTowardCenter:
                        if (!DestinationsFulfilled && CurrentTaskTimeout > 0) break;
                        
                        CurrentState = CurrentStateType.WaitingInCenter;
                        CurrentTaskTimeout = 2000;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void StartMovingTowardCenter()
            {
                CurrentState = CurrentStateType.MovingTowardCenter;
                CurrentTaskTimeout = 1000;
                CurrentMaxStuckDistance = (int)Math.Sqrt(TotalEntities / Math.PI);
                
                foreach (var follower in Followers)
                {
                    follower.SetMovePoint(0, 0, 10);
                }
            }

            private void StartExploding()
            {
                CurrentState = CurrentStateType.Exploding;
                CurrentTaskTimeout = 50;
                CurrentMaxStuckDistance = int.MaxValue;
                var i = 0;
                foreach (var follower in Followers)
                {
                    var angle = 2.0 * 3.1415 * (i++ / (double) Followers.Count);
                    var (x, y) = RandomPointOnCircle(
                        CurrentCenterX,
                        CurrentCenterY,
                        CurrentDistance,
                        angle
                    );
                    follower.SetMovePoint(x, y, 10);
                }
            }

            private void StartMovingTowardExplosion()
            {
                CurrentState = CurrentStateType.MovingToExplosionPoint;
                CurrentTaskTimeout = 1500;
                CurrentMaxStuckDistance = 20;
                CurrentDistance = (int)(Followers.Count / 1.5);

                var minDistanceFromCenter = 1.5 * TotalEntitiesArea;
                do
                {
                    CurrentCenterX = Random.Next(Leader.WorldMinX + CurrentDistance + 1,
                        Leader.WorldMaxX - CurrentDistance - 1);
                    CurrentCenterY = Random.Next(Leader.WorldMinY + CurrentDistance + 1,
                        Leader.WorldMaxY - CurrentDistance - 1);
                } while (Math.Sqrt(Math.Pow(CurrentCenterX, 2) + Math.Pow(CurrentCenterY, 2)) < minDistanceFromCenter);

                foreach (var follower in Followers)
                    follower.SetMovePoint(CurrentCenterX, CurrentCenterY, 3);
            }
        }

        private enum CurrentStateType
        {
            MovingToExplosionPoint,
            Exploding,
            WaitingInCircle,
            MovingTowardCenter,
            WaitingInCenter
        }
    }
}