using System;
using System.Collections.Generic;
using System.Text;

namespace NosCore.PathFinder.Infrastructure
{
    public struct BrushFire
    {
        public BrushFire(Cell originCell, short size, ValuedCell?[,] brushFireGrid)
        {
            OriginCell = originCell;
            Size = size;
            BrushFireGrid = brushFireGrid;
        }

        public Cell OriginCell { get; set; }

        public short Size { get; set; }

        public ValuedCell?[,] BrushFireGrid { get; set; }

        public ValuedCell? this[int x, int y] => BrushFireGrid[x, y];

        public int GetLength(int dimension)
        {
            return BrushFireGrid.GetLength(dimension);
        }
    }
}
