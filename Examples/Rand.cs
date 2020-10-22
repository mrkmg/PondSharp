namespace PondSharp.Examples
{
    /// <summary>
    /// This entity moves randomly.
    /// </summary>
    public class Rand : BaseEntity
    {
        public override void OnCreated()
        {
            ChooseRandomDirection();
        }
        
        public override void Tick()
        {
            if (_random.Next(100) == 0) 
            {
                ChooseRandomDirection();
            }
            MoveTo(X + _forceX, Y + _forceY);
        }
    }
}