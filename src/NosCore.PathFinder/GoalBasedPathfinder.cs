using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder
{
    public class GoalBasedPathfinder : IPathfinder
    {
        private readonly Cell?[,] _brushFire;
        private readonly IMapGrid _mapGrid;

        public GoalBasedPathfinder(Cell?[,] brushFire, IMapGrid mapGrid)
        {
            _brushFire = brushFire;
            _mapGrid = mapGrid;
        }

        public IEnumerable<Cell> FindPath(Cell start, Cell end)
        {
            List<Cell> list = new List<Cell>();
            if (end.X >= _brushFire.GetLength(0) || end.Y >= _brushFire.GetLength(1) ||
                _brushFire[end.X, end.Y] == null || end.X < 0 || end.Y < 0 ||
                start.X >= _brushFire.GetLength(0) || start.Y >= _brushFire.GetLength(1) ||
                _brushFire[start.X, start.Y] == null || start.X < 0 || start.Y < 0)
            {
                return list;
            }

            if (!(_brushFire[end.X, end.Y] is { } currentnode)) return list;
            while (currentnode.Value > 1)
            {
                var newnode = _mapGrid.GetNeighbors(currentnode).Select(s => new Cell(s.X, s.Y, _brushFire[s.X, s.Y]?.Value ?? 0))?.OrderBy(s => s.Value).FirstOrDefault();
                if (newnode is { } cell)
                {
                    list.Add(cell);
                    currentnode = cell;
                }
            }
            return list;
        }
    }
}
