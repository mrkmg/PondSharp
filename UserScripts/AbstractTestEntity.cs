namespace PondSharp.UserScripts
{
    public abstract class AbstractTestEntity
    {
        public string Id;
        protected readonly AbstractEntityController _controller;

        protected AbstractTestEntity(string id, AbstractEntityController controller)
        {
            Id = id;
            _controller = controller;
        }
        
        public int X { get; internal set; }
        public int Y { get; internal set; }

        public abstract void Tick();

        public bool MoveTo(int x, int y) => _controller.MoveTo(this, x, y);
    }
}