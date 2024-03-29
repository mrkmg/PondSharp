using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using PondSharp.Client.Shared;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public sealed class PondManager : IDisposable
    {
        private readonly PondCanvas _pondCanvas;
        private readonly IEntityCreator _entityCreator;
        private readonly PondEngine _pondEngine;
        
        private readonly Timer _tickTimer;
        private DateTime _lastTime = DateTime.Now;
        private static readonly Random Random = new();
        
        public double CurrentTickTime = 1;
        public bool IsRunning => _tickTimer.Enabled;

        public double TickSpeed
        {
            get => _tickTimer.Interval;
            set => _tickTimer.Interval = value;
        }

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas, [NotNull] IEntityCreator entityCreator)
        {
            _tickTimer = new(1000.0/60.0); //60tps 
            _tickTimer.Elapsed += (_, _) => Tick();
            
            _pondCanvas = canvas;
            _entityCreator = entityCreator;
            _pondEngine = engine;

            _pondEngine.EntityAdded += EngineOnEntityAdded;
            _pondEngine.EntityMoved += EngineOnEntityMoved;
            _pondEngine.EntityColorChanged += EngineOnEntityColorChanged;
            _pondEngine.EntityRemoved += EngineOnEntityRemoved;

            _pondCanvas.OnClick += PondCanvasClicked;
        }

        private void PondCanvasClicked(object _, PondCanvas.ClickArgs args)
        {
            if (args.X >= _pondEngine.MinX && args.X <= _pondEngine.MaxX && args.Y >= _pondEngine.MinY && args.Y <= _pondEngine.MaxY) 
                InitializeAndCreateEntity(_entityCreator.CreateSelectedEntity(), new() {X = args.X, Y = args.Y});
        }

        private void Tick()
        {
            var diff = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            CurrentTickTime = diff * 0.95 + CurrentTickTime * 0.05;
            try
            {
                _pondEngine.Tick();
                _pondCanvas.FlushChangeQueue();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Tick Exception: ${e.Message} ${e.StackTrace}");
                _pondCanvas.Clear();
                Stop();
            }

        }
        
        private void EngineOnEntityAdded(object sender, IEntity e)
        {
            _pondCanvas.CreateEntity(e.Id, e.X, e.Y, e.Color);
        }

        private void EngineOnEntityRemoved(object sender, IEntity e)
        {
            _pondCanvas.DestroyEntity(e.Id);
        }

        private void EngineOnEntityMoved(object sender, (int, int) position)
        {
            if (sender is not IEntity entity) return;
            var (x, y) = position;
            _pondCanvas.MoveEntity(entity.Id, x, y);
        }

        private void EngineOnEntityColorChanged(object sender, int color)
        {
            if (sender is not IEntity entity) return;
            _pondCanvas.ChangeEntityColor(entity.Id, color);
        }

        public void Start()
        {
            _lastTime = DateTime.Now;
            _tickTimer.Enabled = true;
        }

        public void Stop()
        {
            _tickTimer.Enabled = false;
        }
        
        private static int RandomColor(float minBrightness = 0.5f)
        {
            Color color;
            do color = Color.FromArgb(Random.Next(255), Random.Next(255), Random.Next(255));
            while (color.GetBrightness() < minBrightness);
            return color.ToArgb();
        }

        public void InitializeAndCreateEntity(Entity entity, EntityOptions initialization)
        {
            _pondEngine.InsertEntity(entity, initialization);
        }

        public void Reset()
        {
            foreach (var instance in _pondEngine.Entities)
            {
                _pondCanvas.DestroyEntity(instance.Id);
            }
            _pondEngine.ClearAllEntities();
        }

        public void Dispose()
        {
            Reset();
            
            _pondEngine.EntityAdded -= EngineOnEntityAdded;
            _pondEngine.EntityMoved -= EngineOnEntityMoved;
            _pondEngine.EntityColorChanged -= EngineOnEntityColorChanged;
            _pondEngine.EntityRemoved -= EngineOnEntityRemoved;

            _pondCanvas.OnClick -= PondCanvasClicked;
            _tickTimer?.Dispose();
        }
    }
}