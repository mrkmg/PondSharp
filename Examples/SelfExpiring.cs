using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// This entity moves randomly.
    /// </summary>
    [PondUserSpawnable(Name = "Burn Effect")]
    public class SelfExpiring : Simple
    {
        private static int StartAgeReal = 80;
        
        [PondAdjustable(Min = 5, Max=240, Name = "Age")]
        private static int StartAge
        {
            get => StartAgeReal;
            // ReSharper disable once UnusedMember.Local
            set
            {
                StartAgeReal = value;
                Colors = CreateAgeColor();
            }
        }

        [PondAdjustable(Min = 0, Max = 1, Step = 0.05)]
        private static double Ratio { get; set; } = 0.8;
        
        [PondAdjustable(Min = 1, Max = 10, Step = 0.2)]
        private static double RespawnRatio { get; set; } = 2.2;
        
        private int _remainingLife;
        private int _age;
        
        protected override void OnCreated()
        {
            base.OnCreated();
            SetIsBlocking(false);
            var mid = (1 - Ratio) * StartAge;
            _age = (int)(Random.NextDouble() <= Ratio ? mid - Math.Pow(mid, Random.NextDouble()) : Math.Pow(StartAge - mid, Random.NextDouble()) + mid);
            _remainingLife = StartAge - _age;
        }

        protected override void Tick()
        {
            // Because the change of the age will
            // change the length of the Colors
            // array, we need to ensure we do not
            // try to change the color past colors
            // that are available.
            if (Colors.Count <= _age)
            {
                Destroy();
                return;
            }

            if (Random.NextDouble() * StartAge * RespawnRatio < 1)
                CreateEntity<SelfExpiring>(new EntityOptions {X = X, Y = Y});
            
            ChangeColor(Colors[_age]);
            _age++;
            if (--_remainingLife <= 0)
            {
                Destroy();
                return;
            }

            // if (Random.Next(10) == 0)
                // ResetPower();
            
            base.Tick();
        }
            
        private static List<int> CreateAgeColor()
        {
            return ColorTransition.From(StartAge, InternalTransitionData.TransitionColors, InternalTransitionData.TransitionRatios)
                .Colors()
                .Select(c => c.ToArgb())
                .ToList();
        }
        
        private static List<int> Colors = CreateAgeColor();
    }

    internal static class InternalTransitionData
    {

        internal static readonly Color[] TransitionColors =
        {
            Color.FromArgb(100, 0, 1), 
            Color.Firebrick, 
            Color.Yellow, 
            Color.FromArgb(50, 0, 1), 
            Color.Black
        };
        internal static readonly double[] TransitionRatios = { 0, 0.3, 0.6, 0.9 };
        
    }
    
}