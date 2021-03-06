using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// All entities follow one master entity.
    /// </summary>
    [PondDefaults(InitialCount = 0, NewCount = 50)]
    public class Chase : BaseEntity
    {
        private static int? MasterEntityId;
        private static (int x, int y) MasterXy = (0, 0);

        private (int x, int y) Target = (0, 0);
        private int FleeCooldown;
        private bool IsMaster => MasterEntityId == Id;
        
        protected override void OnCreated()
        {
            ChangeViewDistance(5);
            if (MasterEntityId != null) {
                return;
            }
            ChangeColor(0xFF0000);
            MasterEntityId = Id;
            Target = (X, Y);
        }

        protected override void OnDestroy()
        {
            if (IsMaster) MasterEntityId = null;
        }

        protected override void Tick()
        {
            if (IsMaster)
            {
                (ForceX, ForceY) = GetForceDirection(Target.x - X, Target.y - Y);
                if (X == Target.x && Y == Target.y || !MoveTo(X + ForceX, Y + ForceY))
                    ChooseRandomTarget();
                MasterXy = (X, Y);
            }
            else
            {
                if (FleeCooldown == 0)
                    (ForceX, ForceY) = GetForceDirection(MasterXy.x - X, MasterXy.y - Y);
                
                CheckFlee();

                if (!MoveTo(X + ForceX, Y + ForceY))
                    (ForceX, ForceY) = (-ForceX, -ForceY);
            }

        }

        private void ChooseRandomTarget()
        {
            Target = (X + Random.Next(WorldMinX, WorldMaxX), Y + Random.Next(WorldMinY, WorldMaxY));
        }

        private void CheckFlee()
        {
            if (FleeCooldown >= 1)
            {
                FleeCooldown--;
                return;
            }
            
            if (!VisibleEntities.Any()) return;
            
            var closestEntity = VisibleEntities
                .OrderBy(e => EntityDist(this, e))
                .First();

            (ForceX, ForceY) = GetForceDirection(X - closestEntity.X, Y - closestEntity.Y);
            if (ForceX == 0 && ForceY == 0) ChooseRandomDirection();
            FleeCooldown = 10;
        }
    }
}