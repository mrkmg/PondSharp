using System;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// The simplest example of an entity.
    /// </summary>
    [PondDefaults(InitialCount = 0, NewCount = 500)]
    public class Simple : BaseEntity
    {
        [PondAdjustable(Min = 1, Max = 100, Name = "Speed")]
        private static int StartingSpeed { get; set; } = 20;
        
        private double _powerX;
        private double _powerY;
        private double _currentX;
        private double _currentY;
        
        protected override void OnCreated()
        {
            ResetPower();
        }
        
        protected override void Tick()
        {
            _currentX += _powerX;
            _currentY += _powerY;
            if (Math.Abs(_currentX) > 1)
            {
                ForceX = (int)_currentX;
                _currentX -= ForceX;
            }
            else
            {
                ForceX = 0;
            }
            if (Math.Abs(_currentY) > 1)
            {
                ForceY = (int)_currentY;
                _currentY -= ForceY;
            }
            else
            {
                ForceY = 0;
            }
            
            if (!MoveTo(X + ForceX, Y + ForceY))
            {
                ResetPower();
            }
        }

        protected void ResetPower()
        {
            var direction = Math.PI * 2 * Random.NextDouble();
            _powerX = Math.Cos(direction) * (StartingSpeed/100.0);
            _powerY = Math.Sin(direction) * (StartingSpeed/100.0);
            ForceX = 0;
            ForceY = 0;
            _currentX = 0;
            _currentY = 0;
        }
    }
}