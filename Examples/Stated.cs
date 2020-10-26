using System;
using System.Collections.Generic;
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
    public class Stated : Entity
    {
        private static readonly Random Random = new Random();
        private int DestinationX;
        private int DestinationY;
        private State MyState;
        private int StuckCounter = 10;
        private bool DidInititalMove = false;

        private static List<State> States = new List<State>();

        public override void OnCreated()
        {
            if (States.Count == 0 || States.All(s => s.IsFull))
            {
                MyState = new State {Leader = this};
                States.Add(MyState);
            }
            else
            {
                MyState = States.First(s => !s.IsFull);
                MyState.Followers.Add(this);
            }

            ChangeColor(MyState.StateColor);
        }

        public override void OnDestroy()
        {
            if (MyState == null || MyState.Leader.Id != Id) return;
            
            MyState.Leader = null;
            foreach (var follower in MyState.Followers)
            {
                follower.MyState = null;
            }
            MyState.Followers.Clear();
            States.Remove(MyState);
        }

        public override void Tick()
        {
            if (MyState == null)
            {
                if (Color != 0xFF0000) 
                    ChangeColor(0xFF0000);
                return;
            }
            
            if (MyState.Leader.Id == Id) MyState.Tick();

            if (X == DestinationX && Y == DestinationY) return;

            var (x, y) = ((double)DestinationX - X, (double)DestinationY - Y);
            var dM = Math.Max(Math.Abs(x), Math.Abs(y));
            var (fx, fy) = dM == 0 ? (0, 0) : ((int)Math.Round(x / dM), (int)Math.Round(y / dM));
            if (!MoveTo(X + fx, Y + fy)) {
                if (DidInititalMove) {
                    if (--StuckCounter <= 0) {
                        DestinationX = X;
                        DestinationY = Y;
                        return;
                    }
                }
                if(MoveTo(X +Random.Next(-1, 2), Y + Random.Next(-1, 2)))
                    DidInititalMove = true;
            } else {
                DidInititalMove = true;
            }
        }

        private class State
        {
            private static (int, int) RandomPointOnCircle(int x, int y, int dist, double? angle = null)
            {
                if (angle == null) angle = 2.0 * 3.1415 * Random.NextDouble();
                return (x + (int)(dist * Math.Sin(angle ?? 0)), y + (int)(dist * Math.Cos(angle ?? 0)));
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
            private CurrentStateType CurrentState { get; set; } = CurrentStateType.Waiting;
            private int CurrentCenterX;
            private int CurrentCenterY;
            private int CurrentDistance;
            private int GroupSize = 5 + Random.Next(20);

            private bool DestinationsFulfilled =>
                !(Leader?.DestinationX != Leader?.X || Leader?.DestinationY != Leader?.Y ||
                  Followers.Any(f => f.X != f.DestinationX || f.Y != f.DestinationY));

            public void Tick()
            {
                switch (CurrentState)
                {
                    case CurrentStateType.Pending:
                    {if (States.Any(s => s.CurrentState == CurrentStateType.Separating)) break;
                        CurrentDistance = 5 + Random.Next(20);
                        CurrentCenterX = Random.Next(Leader.WorldMinX + CurrentDistance + 1, Leader.WorldMaxX - CurrentDistance - 1);
                        CurrentCenterY = Random.Next(Leader.WorldMinY + CurrentDistance + 1, Leader.WorldMaxY - CurrentDistance - 1);
                        Leader.DestinationX = CurrentCenterX;
                        Leader.DestinationY = CurrentCenterY;
                        Leader.StuckCounter = 100;
                        Leader.DidInititalMove = false;

                        foreach (var follower in Followers)
                        {
                            follower.DestinationX = CurrentCenterX;
                            follower.DestinationY = CurrentCenterY;
                            follower.StuckCounter = 100;
                            follower.DidInititalMove = false;
                        }

                        CurrentState = CurrentStateType.Joining;
                        break;
                    }
                    case CurrentStateType.Joining:
                        if (!DestinationsFulfilled) break;
                        
                        var i = 0;
                        foreach (var follower in Followers)
                        {
                            var angle = 2.0 * 3.1415 * ( i++ / (double) Followers.Count );
                            (follower.DestinationX, follower.DestinationY) = RandomPointOnCircle(
                                CurrentCenterX, 
                                CurrentCenterY, 
                                CurrentDistance,
                                angle
                            );
                            follower.StuckCounter = 10;
                            follower.DidInititalMove = false;
                        }
                        CurrentState = CurrentStateType.Exploding;
                            
                        break;
                    case CurrentStateType.Exploding:
                        if (DestinationsFulfilled)
                        {
                            CurrentState = CurrentStateType.Waiting;
                        }

                        break;
                    case CurrentStateType.Waiting:
                        if (States.Any(s => s.CurrentState == CurrentStateType.Exploding || s.CurrentState == CurrentStateType.Joining)) break;
                        
                        Leader.DestinationX = 0;
                        Leader.DestinationY = 0;
                        Leader.StuckCounter = 50;
                        Leader.DidInititalMove = false;
                        foreach (var follower in Followers)
                        {
                            follower.DestinationX = 0;
                            follower.DestinationY = 0;
                            follower.StuckCounter = 50;
                            follower.DidInititalMove = false;
                        }

                        CurrentState = CurrentStateType.Separating;
                        break;
                    case CurrentStateType.Separating:
                        if (DestinationsFulfilled)
                            CurrentState = CurrentStateType.Pending;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }            
        }

        private enum CurrentStateType
        {
            Joining,
            Exploding,
            Waiting,
            Separating,
            Pending
        }
    }
}