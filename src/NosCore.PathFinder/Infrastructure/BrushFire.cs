namespace NosCore.PathFinder.Infrastructure
{
    public readonly struct BrushFire
    {
        public BrushFire(Cell originCell, short size, ValuedCell?[,] brushFireGrid)
        {
            OriginCell = originCell;
            Size = size;
            ValuedCellGrid = brushFireGrid;
        }

        private ValuedCell?[,] ValuedCellGrid { get; }

        public Cell OriginCell { get; }

        public short Size { get; }

        public ValuedCell? this[int x, int y] => ValuedCellGrid[x, y];

        public int GetLength(int dimension)
        {
            return ValuedCellGrid.GetLength(dimension);
        }
    }
}
