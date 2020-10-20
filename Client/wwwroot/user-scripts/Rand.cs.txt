namespace PondSharp.Examples
{
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