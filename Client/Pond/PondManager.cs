using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Timers;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public sealed class PondManager
    {
        private readonly PondCanvas _pondCanvas;
        private readonly IEntityCreator _entityCreator;
        private readonly PondEngine _pondEngine;
        
        private readonly Timer _tickTimer;
        private DateTime _lastTime = DateTime.Now;
        private static readonly Random Random = new();
        private int _nextId;
        
        public double CurrentTickTime = 1;
        public bool IsRunning => _tickTimer.Enabled;

        public double TickSpeed
        {
            get => _tickTimer.Interval;
            set => _tickTimer.Interval = value;
        }

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas, [NotNull] IEntityCreator entityCreator)
        {
            _tickTimer = new(16); //60tps 
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
            if (args.X >= _pondEngine.MinX && args.X <= _pondEngine.MaxX && args.Y >= _pondEngine.MinY && args.Y <= _pondEngine.MaxY) InitializeAndCreateEntity(_entityCreator.CreateSelectedEntity(), args.X, args.Y);
        }

        private void Tick()
        {
            var diff = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            CurrentTickTime = diff * 0.99 + CurrentTickTime * 0.01;
            try
            {
                _pondEngine.Tick();
                _pondCanvas.FlushChangeQueue();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Tick Exception: ${e.Message} ${e.StackTrace}");
                _pondCanvas.Clear();
                Stop().RunSynchronously();
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

        public async Task Start()
        {
            _lastTime = DateTime.Now;
            _tickTimer.Enabled = true;
            await _pondCanvas.Start().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            _tickTimer.Enabled = false;
            await _pondCanvas.Stop().ConfigureAwait(false);
        }
        
        private static int RandomColor(float minBrightness = 0.5f)
        {
            Color color;
            do color = Color.FromArgb(Random.Next(255), Random.Next(255), Random.Next(255));
            while (color.GetBrightness() < minBrightness);
            return color.ToArgb();
        }

        public void InitializeAndCreateEntity(Entity entity, int? x = null, int? y = null)
        {
            int xPos;
            int yPos;
            if (x == null || y == null)
            {
                do
                {
                    xPos = Random.Next(_pondEngine.MinX, _pondEngine.MaxX);
                    yPos = Random.Next(_pondEngine.MinY, _pondEngine.MaxY);
                } while (_pondEngine.GetEntityAt(xPos, yPos) != null);
            }
            else
            {
                xPos = x.Value;
                yPos = y.Value;
            }
            _pondEngine.InsertEntity(entity, _nextId++, xPos, yPos, RandomColor());
        }

        public void Reset()
        {
            foreach (var instance in _pondEngine.Entities)
            {
                _pondCanvas.DestroyEntity(instance.Id);
            }
            _pondEngine.ClearAllEntities();
        }
    }
}