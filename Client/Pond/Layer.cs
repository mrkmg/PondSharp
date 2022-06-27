using System;
using System.Collections.Generic;

namespace PondSharp.Client.Pond
{
    public sealed class Layer<T>
    {
        private const int BlockSize = 15;
        
        public int MinX { get; }
        public int MaxX { get; }
        public int MinY { get; }
        public int MaxY { get; }
        
        private readonly int _blocksWide;
        private T[][] _objectPositions;
        private HashSet<T>[] _objectBlocks;
        
        public Layer(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            
            _blocksWide = (int) Math.Ceiling((double)(MaxX - MinX) / BlockSize);

            _objectPositions = new T[MaxX - MinX + 1][];
            for (var x = 0; x <= MaxX - MinX; x++)
            {
                _objectPositions[x] = new T[MaxY - MinY + 1];
                for (var y = 0; y <= MaxY - MinY; y++)
                    _objectPositions[x][y] = default;
            }

            _objectBlocks = new HashSet<T>[GetBlock(MaxX, MaxY) + 1];
            for (var i = _objectBlocks.Length - 1; i >= 0; i--)
                _objectBlocks[i] = new();
        }
        
        private int GetBlock(int x, int y) => 
            (x - MinX) / BlockSize + (y - MinY) / BlockSize * _blocksWide;
      
        public void Add(T obj, int x, int y)
        {
            _objectBlocks[GetBlock(x, y)].Add(obj);
            _objectPositions[x - MinX][y - MinY] = obj;
        }

        public void Move(T obj, int fromX, int fromY, int toX, int toY)
        {
            _objectPositions[fromX - MinX][fromY - MinY] = default;
            _objectPositions[toX - MinX][toY - MinY] = obj;
            
            var oldBlock = GetBlock(fromX, fromY);
            var newBlock = GetBlock(toX, toY);
            
            if (oldBlock == newBlock) return;
            _objectBlocks[oldBlock].Remove(obj);
            _objectBlocks[newBlock].Add(obj);
        }

        public void Remove(T obj, int x, int y)
        {
            _objectBlocks[GetBlock(x, y)].Remove(obj);
            _objectPositions[x - MinX][y - MinY] = default;
        }

        public T GetAt(int x, int y) => _objectPositions[x - MinX][y - MinY];

        public IEnumerable<T> GetNear(int centerX, int centerY, int dist)
        {
            for (var x = centerX - dist; x <= centerX + dist + BlockSize; x += BlockSize)
            for (var y = centerY - dist; y <= centerY + dist + BlockSize; y += BlockSize)
                if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
                    foreach (var entity in _objectBlocks[GetBlock(x, y)])
                        yield return entity;
        }
    }
}