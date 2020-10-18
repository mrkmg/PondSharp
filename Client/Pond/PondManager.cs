using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Timers;
using PondSharp.UserScripts;

namespace PondSharp.Client.Pond
{
    public class PondManager
    {
        public readonly PondCanvas PondCanvas;
        public readonly PondEngine PondEngine;
        
        private readonly Timer _tickTimer;
        private DateTime _lastTime = DateTime.Now;
        public double CurrentTickTime = 50;

        public PondManager([NotNull] PondEngine engine, [NotNull] PondCanvas canvas)
        {
            _tickTimer = new Timer(1000 / 20); // 20 tps
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
            foreach (var entity in PondEngine.Entities)
                try
                {
                    entity.Tick();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Tick Exception: ${e.Message}");
                }
            PondCanvas.FlushChangeQueue();
        }
        
        private void EngineOnEntityAdded(object sender, AbstractEntity e)
        {
            PondCanvas.CreateEntity(e.Id, e.X, e.Y, e.Color);
        }

        private void EngineOnEntityMoved(object sender, (int, int) position)
        {
            if (!(sender is AbstractEntity entity)) return;
            var (x, y) = position;
            PondCanvas.QueueMoveEntity(entity.Id, x, y);
        }

        private void EngineOnEntityColorChanged(object sender, int color)
        {
            if (!(sender is AbstractEntity entity)) return;
            PondCanvas.QueueChangeEntityColor(entity.Id, color);
        }

        public async Task Start()
        {
            _lastTime = DateTime.Now;
            _tickTimer.Enabled = true;
            await PondCanvas.Start();
        }

        public async Task Stop()
        {
            _tickTimer.Enabled = false;
            await PondCanvas.Stop();
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