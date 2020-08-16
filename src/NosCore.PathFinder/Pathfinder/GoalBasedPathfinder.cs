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

        private GoalBasedNode? GetParent(ValuedCell currentnode, BrushFire brushFire, GoalBasedNode?[,] brushFireNodes)
        {
            var newnode = _mapGrid.GetNeighbors(currentnode).Select(s => new ValuedCell(s.X, s.Y, brushFire[s.X, s.Y]?.Value ?? 0))?.OrderBy(s => s.Value).FirstOrDefault();
            if (newnode is { } cell)
            {
                var currentNode = new GoalBasedNode(cell, null);
                brushFireNodes[cell.X, cell.Y] ??= currentNode;

                if (cell.Value > 1)
                {
                    currentNode.Parent ??= GetParent(cell, brushFire, brushFireNodes);
                }

                return brushFireNodes[cell.X, cell.Y];
            }

            return null;
        }
        private GoalBasedNode?[,] CacheBrushFire(BrushFire brushFire)
        {

            GoalBasedNode?[,] brushFireNodes = new GoalBasedNode?[brushFire.GetLength(1), brushFire.GetLength(0)];
            for (short y = 0; y < brushFire.GetLength(1); y++)
            {
                for (short x = 0; x < brushFire.GetLength(0); x++)
                {
                    if (!(brushFire[x, y] is { } currentnode))
                    {
                        continue;
                    }

                    GetParent(currentnode, brushFire, brushFireNodes);
                }
            }
            _brushFirecache.Set(brushFire.OriginCell, brushFireNodes, DateTimeOffset.Now.AddSeconds(10));
            return brushFireNodes;
        }

        public IEnumerable<Cell> FindPath(Cell start, Cell end)
        {
            List<Cell> list = new List<Cell>();
            _brushFirecache.TryGetValue(start, out GoalBasedNode?[,]? brushFireOut);
            if (brushFireOut == null)
            {
                var brushFire = _mapGrid.LoadBrushFire(start, _heuristic);
                brushFireOut = CacheBrushFire(brushFire);
            }
            if (brushFireOut == null || end.X >= brushFireOut.GetLength(0) || end.Y >= brushFireOut.GetLength(1) ||
                brushFireOut[end.X, end.Y] == null || end.X < 0 || end.Y < 0 ||
                start.X >= brushFireOut.GetLength(0) || start.Y >= brushFireOut.GetLength(1) ||
                brushFireOut[start.X, start.Y] == null || start.X < 0 || start.Y < 0)
            {
                return list;
            }

            if (!(brushFireOut[end.X, end.Y] is { } currentnode)) return list;
            while (currentnode.Parent != null)
            {
                list.Add(currentnode.Parent.Cell);
                currentnode = currentnode.Parent;
            }

            list.Reverse();
            return list;
        }
    }
}
