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
        public readonly PondCanvas PondCanvas;
        public readonly PondEngine PondEngine;
        
        private readonly Timer _tickTimer;
        private DateTime _lastTime = DateTime.Now;
        private readonly Random _random = new Random();
        private int _nextId = 0;
        
        public double CurrentTickTime = 1;
        public bool IsRunning => _tickTimer.Enabled;

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas)
        {
            _tickTimer = new Timer(16); // 60 tps (1000/60)
            _tickTimer.Elapsed += (sender, args) => Tick();
            
            PondCanvas = canvas;
            PondEngine = engine;

            PondEngine.EntityAdded += EngineOnEntityAdded;
            PondEngine.EntityMoved += EngineOnEntityMoved;
            PondEngine.EntityColorChanged += EngineOnEntityColorChanged;
        }
        
        private void Tick()
        {
            var diff = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            CurrentTickTime = diff * 0.99 + CurrentTickTime * 0.01;
            try
            {
                foreach (var entity in PondEngine.Entities)
                    entity.Tick();
                PondCanvas.FlushChangeQueue();
            }
            catch (Exception e)
            {
                PondCanvas.ClearChangeQueue();
                Console.WriteLine($"Tick Exception: ${e.Message}");
                Stop().RunSynchronously();
            }

        }
        
        private void EngineOnEntityAdded(object sender, IEntity e)
        {
            PondCanvas.CreateEntity(e.Id, e.X, e.Y, e.Color);
            e.OnCreated();
        }

        private void EngineOnEntityMoved(object sender, (int, int) position)
        {
            if (!(sender is IEntity entity)) return;
            var (x, y) = position;
            PondCanvas.QueueMoveEntity(entity.Id, x, y);
        }

        private void EngineOnEntityColorChanged(object sender, int color)
        {
            if (!(sender is IEntity entity)) return;
            PondCanvas.QueueChangeEntityColor(entity.Id, color);
        }

        public async Task Start()
        {
            _lastTime = DateTime.Now;
            _tickTimer.Enabled = true;
            await PondCanvas.Start().ConfigureAwait(false);
        }

        public async Task Stop()
        {
            _tickTimer.Enabled = false;
            await PondCanvas.Stop().ConfigureAwait(false);
        }

        public void InitializeAndCreateEntity(IEntity entity)
        {
            int ColorRnd(int min) => _random.Next(min) + (0xFF - min);
            entity.Initialize(
                _nextId++,
                PondEngine,
                _random.Next(-PondCanvas.Width/2, PondCanvas.Width/2-1), 
                _random.Next(-PondCanvas.Height/2, PondCanvas.Height/2-1),
                Color.FromArgb(ColorRnd(0x66), ColorRnd(0x66), ColorRnd(0x66)).ToArgb());
            PondEngine.InsertEntity(entity);
        }

        public void Reset()
        {
            foreach (var instance in PondEngine.Entities)
            {
                PondCanvas.DestroyEntity(instance.Id);
            }
            PondEngine.ClearAllEntities();
        }
    }
}