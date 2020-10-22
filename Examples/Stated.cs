using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class Stated : AbstractEntity
    {
        private int DestinationX = 0;
        private int DestinationY = 0;
        private State MyState;

        private static List<State> States = new List<State>();

        public override void OnCreated()
        {
            if (States.Count == 0 || !States.Any(s => !s.IsFull))
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

            var (x, y) = (DestinationX - X, DestinationY - Y);
            var dM = Math.Max(Math.Abs(x), Math.Abs(y));
            (x, y) = dM == 0 ? (0, 0) : ((int)Math.Round((double)x / dM), (int)Math.Round((double)y / dM));
            MoveTo(X + x, Y + y);
        }

        private class State
        {
            private static readonly Random Random = new Random();
            private static (int, int) RandomPointOnCircle(int x, int y, int dist)
            {
                var angle = 2.0 * 3.1415 * Random.NextDouble();
                return (x + (int)(dist * Math.Sin(angle)), y + (int)(dist * Math.Cos(angle)));
            }

            public Stated Leader { get; set; }
            public List<Stated> Followers { get; } = new List<Stated>();
            public readonly int StateColor = System.Drawing.Color.FromArgb(Random.Next(0x77) + 0x88, Random.Next(0x77) + 0x88, Random.Next(0x77) + 0x88).ToArgb();
            public bool IsFull => Followers.Count >= 20;
            private CurrentStateType CurrentState { get; set; } = CurrentStateType.Pending;
            private int Cooldown = 0;

            private bool DestinationsFulfilled =>
                !(Leader?.DestinationX != Leader?.X || Leader?.DestinationY != Leader?.Y ||
                  Followers.Any(f => f.X != f.DestinationX || f.Y != f.DestinationY));

            public void Tick()
            {
                switch (CurrentState)
                {
                    case CurrentStateType.Pending:
                    {
                        Leader.DestinationX = Random.Next(Leader.WorldMinX + 20, Leader.WorldMaxX - 20);
                        Leader.DestinationY = Random.Next(Leader.WorldMinY + 20, Leader.WorldMaxY - 20);

                        foreach (var follower in Followers)
                        {
                            (follower.DestinationX, follower.DestinationY) = RandomPointOnCircle(
                                Leader.DestinationX, 
                                Leader.DestinationY, 
                                15
                            );
                        }

                        CurrentState = CurrentStateType.Joining;
                        break;
                    }
                    case CurrentStateType.Joining:
                        if (DestinationsFulfilled)
                        {
                            CurrentState = CurrentStateType.Waiting;
                            Cooldown = Random.Next(160) + 60;
                        }
                            
                        break;
                    case CurrentStateType.Waiting:
                        if (Cooldown > 0)
                        {
                            Cooldown--;
                            return;
                        }
                        
                        Leader.DestinationX = Random.Next(Leader.WorldMinX, Leader.WorldMaxX);
                        Leader.DestinationY = Random.Next(Leader.WorldMinY, Leader.WorldMaxY);
                        foreach (var follower in Followers)
                        {
                            follower.DestinationX = Random.Next(Leader.WorldMinX, Leader.WorldMaxX);
                            follower.DestinationY = Random.Next(Leader.WorldMinY, Leader.WorldMaxY);
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
            Waiting,
            Separating,
            Pending
        }
    }
}