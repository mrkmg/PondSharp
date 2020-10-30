using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// This entity moves randomly.
    /// </summary>
    [PondDefaults(InitialCount = 0, NewCount = 500)]
    public class Rand : BaseEntity
    {
        protected override void OnCreated()
        {
            ChooseRandomDirection();
        }
        
        protected override void Tick()
        {
            if (Random.Next(100) == 0) 
            {
                ChooseRandomDirection();
            }
            MoveTo(X + ForceX, Y + ForceY);
        }
    }
}