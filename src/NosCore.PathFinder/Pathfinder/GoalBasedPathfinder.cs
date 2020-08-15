using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NosCore.PathFinder.Infrastructure;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Pathfinder
{
    public class GoalBasedPathfinder : IPathfinder
    {
        private readonly IMapGrid _mapGrid;
        private readonly MemoryCache _brushFirecache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHeuristic _heuristic;

        public GoalBasedPathfinder(BrushFire brushFire, IMapGrid mapGrid, IHeuristic heuristic)
        {
            _mapGrid = mapGrid;
            _heuristic = heuristic;
            CacheBrushFire(brushFire);
        }

        private void CacheBrushFire(BrushFire brushFire)
        {
            _brushFirecache.Remove(brushFire.OriginCell);
            _brushFirecache.Set(brushFire.OriginCell, brushFire, DateTimeOffset.Now.AddSeconds(10));
        }

        public IEnumerable<Cell> FindPath(Cell start, Cell end)
        {
            List<Cell> list = new List<Cell>();
            _brushFirecache.TryGetValue(start, out BrushFire? brushFireOut);
            if (!(brushFireOut is {} brushFire))
            {
                brushFire = _mapGrid.LoadBrushFire(start, _heuristic);
                CacheBrushFire(brushFire);
            }
            if (end.X >= brushFire.GetLength(0) || end.Y >= brushFire.GetLength(1) ||
                brushFire[end.X, end.Y] == null || end.X < 0 || end.Y < 0 ||
                start.X >= brushFire.GetLength(0) || start.Y >= brushFire.GetLength(1) ||
                brushFire[start.X, start.Y] == null || start.X < 0 || start.Y < 0)
            {
                return list;
            }

            if (!(brushFire[end.X, end.Y] is { } currentnode)) return list;
            while (currentnode.Value > 1)
            {
                var newnode = _mapGrid.GetNeighbors(currentnode).Select(s => new ValuedCell(s.X, s.Y, brushFire[s.X, s.Y]?.Value ?? 0))?.OrderBy(s => s.Value).FirstOrDefault();
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
