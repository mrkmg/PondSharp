namespace PondSharp.Examples
{
    /// <summary>
    /// The simplest example of an entity.
    /// </summary>
    public class Simple : BaseEntity
    {
        public override void OnCreated()
        {
            if (Random.Next(2) == 0)
                ForceX = Random.Next(2) == 0 ? 1 : -1;
            else
                ForceY = Random.Next(2) == 0 ? 1 : -1;
        }
        
        public override void Tick()
        {
            if (!MoveTo(X + ForceX, Y + ForceY))
            {
                ForceX = -ForceX;
                ForceY = -ForceY;
            }
        }
    }
}