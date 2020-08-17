using System.Collections.Generic;

namespace NosCore.PathFinder.Brushfire
{
    public readonly struct BrushFire
    {

        public BrushFire((short X, short Y) origin, short size, Dictionary<(short X, short Y), BrushFireNode?> brushFireGrid, short length,
            short width)
        {
            Origin = origin;
            Size = size;
            Grid = brushFireGrid;
            Length = length;
            Width = width;
        }

        public (short X, short Y) Origin { get; }

        public Dictionary<(short X, short Y), BrushFireNode?> Grid { get; }

        public short Size { get; }

        public short Length { get; }

        public short Width { get; }

        public double? this[short x, short y] => Grid.ContainsKey((x, y)) ? Grid[(x, y)]?.Value : null;
    }
}
