using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using PondSharp.UserScripts;

namespace PondSharp.Examples
{
    /// <summary>
    /// A base entity with some helper classes for the
    /// examples.
    /// </summary>
    public abstract class BaseEntity : Entity
    {
        protected int ForceX;
        protected int ForceY;
        
        protected void ChooseRandomDirection()
        {
            do
            {
                ForceX = Random.Next(-1, 2);
                ForceY = Random.Next(-1, 2);
            } while (ForceX == 0 && ForceY == 0);
        }

        protected static (int, int) GetForceDirection(int x, int y)
        {
            var dM = Math.Max(Math.Abs(x), Math.Abs(y));
            return dM == 0 ? (0, 0) : ((int)Math.Round((double)x / dM), (int)Math.Round((double)y / dM));
        }
        
        protected static int EntityDist(IEntity e1, IEntity e2) => 
            Dist(e1.X, e1.Y, e2.X, e2.Y);
        
        protected static int Dist(int x1, int y1, int x2, int y2) =>
            (int) Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

    }
    
    public readonly struct ColorTransition
    {
        private readonly Color[] _colors;
        private readonly int[] _sizes;
        
        private ColorTransition(Color[] colors, int[] sizes)
        {
            _colors = colors;
            _sizes = sizes;
        }

        public IEnumerable<Color> Colors()
        {
            for (var i = 0; i < _sizes.Length; i++)
            {
                foreach (var color in GenerateSequence(_colors[i], _colors[i + 1], i == 0 ? _sizes[i] : _sizes[i] + 1).Skip(i == 0 ? 0 : 1))
                    yield return color;
            }
        }

        public static ColorTransition From(int totalLength, Color[] colors)
        {
            Debug.Assert(colors.Length >= 2);
            var lengthEach = totalLength / (colors.Length - 1);
            var sizes = new int[colors.Length - 1];
            var i = 0;
            for (; i < sizes.Length; i++) 
                sizes[i] = lengthEach;

            var usedLength = lengthEach * (colors.Length - 1);
            if (usedLength != totalLength) 
                sizes[i - 1] += totalLength - usedLength;

            return new ColorTransition(colors, sizes);
        }

        public static ColorTransition From(int totalLength, Color[] colors, double[] ratios)
        {
            Debug.Assert(colors.Length >= 2);
            Debug.Assert(colors.Length - 1 == ratios.Length);
            var sizes = new int[ratios.Length];
            var i = 0;
            for (; i < sizes.Length - 1; i++) 
                sizes[i] = (int) ((ratios[i + 1] - ratios[i]) * totalLength);
            sizes[i] = (int) ((1.0 - ratios[i]) * totalLength);
            return new ColorTransition(colors, sizes);
        }
        
        private static IEnumerable<Color> GenerateSequence(Color start, Color end, int colorCount)
        {
            for (var n = 0; n < colorCount; n++)
            {
                var ratio = n / (double)(colorCount - 1);
                var negativeRatio = 1.0 - ratio;
                var aComponent = negativeRatio * start.A + ratio * end.A;
                var rComponent = negativeRatio * start.R + ratio * end.R;
                var gComponent = negativeRatio * start.G + ratio * end.G;
                var bComponent = negativeRatio * start.B + ratio * end.B;

                yield return Color.FromArgb((byte)aComponent, (byte)rComponent, (byte)gComponent, (byte)bComponent);
            }
        }
    }
}