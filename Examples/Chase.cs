using System.Linq;

namespace PondSharp.Examples
{
    /// <summary>
    /// All entities follow one master entity.
    /// </summary>
    public class Chase : BaseEntity
    {
        private static int? MasterEntityId = null;
        private static (int x, int y) MasterXy = (0, 0);
        
        private (int x, int y) _target = (0, 0);
        private int _fleeCooldown;
        private bool IsMaster => MasterEntityId == Id;
        
        public override void OnCreated()
        {
            ChangeViewDistance(5);
            if (MasterEntityId != null) {
                return;
            }
            ChangeColor(0xFF0000);
            MasterEntityId = Id;
            _target = (X, Y);
        }

        public override void OnDestroy()
        {
            if (IsMaster) MasterEntityId = null;
        }

        public override void Tick()
        {
            if (IsMaster)
            {
                (_forceX, _forceY) = GetForceDirection(_target.x - X, _target.y - Y);
                if (X == _target.x && Y == _target.y || !MoveTo(X + _forceX, Y + _forceY))
                    ChooseRandomTarget();
                MasterXy = (X, Y);
            }
            else
            {
                if (_fleeCooldown == 0)
                    (_forceX, _forceY) = GetForceDirection(MasterXy.x - X, MasterXy.y - Y);
                
                CheckFlee();

                if (!MoveTo(X + _forceX, Y + _forceY))
                    (_forceX, _forceY) = (-_forceX, -_forceY);
            }

        }

        private void ChooseRandomTarget()
        {
            _target = (X + _random.Next(WorldMinX, WorldMaxX), Y + _random.Next(WorldMinY, WorldMaxY));
        }

        private void CheckFlee()
        {
            if (_fleeCooldown >= 1)
            {
                _fleeCooldown--;
                return;
            }
            
            if (!VisibleEntities.Any()) return;
            
            var closestEntity = VisibleEntities
                .OrderBy(e => EntityDist(this, e))
                .First();

            (_forceX, _forceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (_forceX == 0 && _forceY == 0) ChooseRandomDirection();
            _fleeCooldown = 10;
        }
    }
}