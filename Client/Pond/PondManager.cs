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
        private readonly Random _random = new Random();
        private int _nextId;
        
        public double CurrentTickTime = 1;
        public bool IsRunning => _tickTimer.Enabled;

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas)
        {
            _tickTimer = new Timer(16); // 60 tps (1000/60)
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
                foreach (var entity in _pondEngine.Entities)
                    entity.Tick();
                _pondCanvas.FlushChangeQueue();
            }
            catch (Exception e)
            {
                _pondCanvas.ClearChangeQueue();
                Console.WriteLine($"Tick Exception: ${e.Message}");
                Stop().RunSynchronously();
            }

        }
        
        private void EngineOnEntityAdded(object sender, IEntity e)
        {
            _pondCanvas.CreateEntity(e.Id, e.X, e.Y, e.Color);
            e.OnCreated();
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

        public void InitializeAndCreateEntity(IEntity entity)
        {
            int ColorRnd(int min) => _random.Next(min) + (0xFF - min);
            entity.Initialize(
                _nextId++,
                _pondEngine,
                _random.Next(-_pondCanvas.Width/2, _pondCanvas.Width/2-1), 
                _random.Next(-_pondCanvas.Height/2, _pondCanvas.Height/2-1),
                Color.FromArgb(ColorRnd(0x66), ColorRnd(0x66), ColorRnd(0x66)).ToArgb());
            _pondEngine.InsertEntity(entity);
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