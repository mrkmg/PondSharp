using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// This entity moves randomly.
    /// </summary>
    [PondDefaults(Name = "Burn Effect")]
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
        
        private int _remainingLife;
        private int _age;
        
        protected override void OnCreated()
        {
            base.OnCreated();
            _age = 0;
            _remainingLife = StartAge;
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
            
            ChangeColor(Colors[_age]);
            _age++;
            if (--_remainingLife <= 0)
            {
                Destroy();
                return;
            }

            if (Random.Next(10) == 0)
                ResetPower();
            
            base.Tick();

        }
        
        private const double Break1 = 0.4;
        private const double Break2 = 0.4;
        private static readonly Color Color1 = System.Drawing.Color.FromArgb(unchecked((int)0xff4f0711));
        private static readonly Color Color2= System.Drawing.Color.Firebrick;
        private static readonly Color Color3 = System.Drawing.Color.Yellow;
        private static readonly Color Color4 = System.Drawing.Color.Black;
        private static List<int> Colors = CreateAgeColor();
        
        private static List<int> CreateAgeColor()
        {
            var break1 = (int)(StartAge * Break1);
            var break2 = (int)(StartAge * Break2);
            var break3 = StartAge - (break1 + break2);
            
            return GenerateSequence(Color1, Color2, break1)
                .Concat(GenerateSequence(Color2, Color3, break2 + 1).Skip(1))
                .Concat(GenerateSequence(Color3, Color4, break3 + 1).Skip(1))
                .Select(x => x.ToArgb())
                .ToList();
        }
        
        private static List<Color> GenerateSequence(Color start, Color end, int colorCount)
        {
            var ret = new List<Color>();
            for (var n = 0; n < colorCount; n++)
            {
                var ratio = n / (double)(colorCount - 1);
                var negativeRatio = 1.0 - ratio;
                var aComponent = negativeRatio * start.A + ratio * end.A;
                var rComponent = negativeRatio * start.R + ratio * end.R;
                var gComponent = negativeRatio * start.G + ratio * end.G;
                var bComponent = negativeRatio * start.B + ratio * end.B;

                ret.Add(System.Drawing.Color.FromArgb((byte)aComponent, (byte)rComponent, (byte)gComponent, (byte)bComponent));
            }

            return ret;
        }
    }
    
    
}