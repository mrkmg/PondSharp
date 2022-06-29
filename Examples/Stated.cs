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
    [PondDefaults(InitialCount = 500, NewCount = 100)]
    public class Stated : Entity
    {
        private int _destinationX;
        private int _destinationY;
        private int _stuckCounter;
        private State _myState;
        private int _fleeX;
        private int _fleeY;
        private int _fleeCooldown;

        private static List<State> States = new List<State>();
        private static int TotalEntities => States.Sum(s => s.Followers.Count + 1);
        private static int TotalEntitiesArea => (int) Math.Sqrt(TotalEntities / Math.PI);

        protected override void OnCreated()
        {
            _stuckCounter = 10;
            _destinationX = X;
            _destinationY = Y;

            _myState = States.FirstOrDefault(s => !s.IsFull);
            if (_myState == null)
            {
                _myState = new State {Leader = this};
                States.Add(_myState);
            }
            
            _myState.Followers.Add(this);

            foreach (var state in States)
            {
                state.CurrentState = CurrentStateType.Exploding;
            }
            ChangeColor(_myState.StateColor);
        }

        protected override void OnDestroy()
        {
            if (_myState == null) return;
            if (_myState.Leader?.Id == Id) _myState.Leader = null;
            _myState.Followers.Remove(this);
            if (_myState.Followers.Count == 0) States.Remove(_myState);
            _myState = null;
        }

        protected override void Tick()
        {
            if (_myState == null)
            {
                if (Color != 0xFF0000) 
                    ChangeColor(0xFF0000);
                return;
            }
            
            if (_myState.Leader.Id == Id) _myState.Tick();

            if (X == _destinationX && Y == _destinationY) return;

            if (_fleeCooldown > 0)
            {
                _fleeCooldown--;
                var t = Random.Next(9);
                if (t == 0) MoveTo(X - _fleeX, Y - _fleeY);
                else if (t < 5)  MoveTo(X + _fleeY, Y - _fleeX);
                else MoveTo(X - _fleeY, Y + _fleeX);
                return;
            }

            if (MoveTowardsTarget()) return;
            
            var (x, y) = ((double)_destinationX - X, (double)_destinationY - Y);
            var dM = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            var (fx, fy) = dM == 0 ? (0, 0) : ((int)Math.Round(x / dM), (int)Math.Round(y / dM));
            
            if (dM <= _myState.CurrentMaxStuckDistance && --_stuckCounter <= 0)
            {
                _destinationX = X;
                _destinationY = Y;
                return;
            }

            _fleeCooldown = 3;
            _fleeX = fx;
            _fleeY = fy;
        }

        private void SetMovePoint(int x, int y, int maxStuckCount)
        {
            _destinationX = x;
            _destinationY = y;
            _stuckCounter = maxStuckCount;
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
                !(Leader?._destinationX != Leader?.X || Leader?._destinationY != Leader?.Y ||
                  Followers.Any(f => f.X != f._destinationX || f.Y != f._destinationY));

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
                CurrentMaxStuckDistance = (int)(1.3 * Math.Sqrt(TotalEntities / Math.PI));
                
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