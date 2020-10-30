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
        private readonly PondEngine _pondEngine;
        
        private readonly Timer _tickTimer;
        private DateTime _lastTime = DateTime.Now;
        private static readonly Random Random = new Random();
        private int _nextId;
        
        public double CurrentTickTime = 1;
        public bool IsRunning => _tickTimer.Enabled;

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas)
        {
            _tickTimer = new Timer(16); // 60 tps (1000/60)
            //_tickTimer = new Timer(33); // 30 tps (1000/30)
            _tickTimer.Elapsed += (sender, args) => Tick();
            
            _pondCanvas = canvas;
            _pondEngine = engine;

            _pondEngine.EntityAdded += EngineOnEntityAdded;
            _pondEngine.EntityMoved += EngineOnEntityMoved;
            _pondEngine.EntityColorChanged += EngineOnEntityColorChanged;
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
                _pondCanvas.ClearChangeQueue();
                Stop().RunSynchronously();
            }

        }
        
        private void EngineOnEntityAdded(object sender, IEntity e)
        {
            _pondCanvas.CreateEntity(e.Id, e.X, e.Y, e.Color);
        }

        private void EngineOnEntityMoved(object sender, (int, int) position)
        {
            if (!(sender is IEntity entity)) return;
            var (x, y) = position;
            _pondCanvas.MoveEntity(entity.Id, x, y);
        }

        private void EngineOnEntityColorChanged(object sender, int color)
        {
            if (!(sender is IEntity entity)) return;
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

        public void InitializeAndCreateEntity(Entity entity)
        {
            int xPos;
            int yPos;
            do
            {
                xPos = Random.Next(_pondEngine.MinX, _pondEngine.MaxX);
                yPos = Random.Next(_pondEngine.MinY, _pondEngine.MaxY);
            } while (_pondEngine.GetEntityAt(xPos, yPos) != null);
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