using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// The simplest example of an entity.
    /// </summary>
    [PondDefaults(InitialCount = 0, NewCount = 500)]
    public class Simple : BaseEntity
    {
        protected override void OnCreated()
        {
            if (Random.Next(2) == 0)
                ForceX = Random.Next(2) == 0 ? 1 : -1;
            else
                ForceY = Random.Next(2) == 0 ? 1 : -1;
        }
        
        protected override void Tick()
        {
            if (!MoveTo(X + ForceX, Y + ForceY))
            {
                ForceX = -ForceX;
                ForceY = -ForceY;
            }
        }
    }
}