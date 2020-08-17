using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using NosCore.PathFinder.Brushfire;
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

        private BrushFireNode? GetParent((short X, short Y) currentnode, BrushFire brushFire, BrushFireNode?[,] brushFireNodes, int maxDepth)
        {
            if (maxDepth == 0)
            {
                return null;
            }

            var newnode = _mapGrid.GetNeighbors(currentnode).Select(s => new BrushFireNode((s.X, s.Y), brushFire[s.X, s.Y] ?? 0))?.OrderBy(s => s.Value).FirstOrDefault(s => IsInBoundaries(s.Position, brushFireNodes));
            if (newnode is { } cell)
            {
                var currentNode = new BrushFireNode(cell.Position, null);
                brushFireNodes[cell.Position.X, cell.Position.Y] ??= currentNode;

                if (cell.Value > 1)
                {
                    currentNode.Parent ??= GetParent(cell.Position, brushFire, brushFireNodes, maxDepth - 1);
                }

                return brushFireNodes[cell.Position.X, cell.Position.Y];
            }

            return null;
        }
        private BrushFireNode?[,] CacheBrushFire(BrushFire brushFire)
        {
            BrushFireNode?[,] brushFireNodes = new BrushFireNode?[brushFire.Width, brushFire.Length];
            for (short y = 0; y < brushFire.Width; y++)
            {
                for (short x = 0; x < brushFire.Length; x++)
                {
                    if (!(brushFire[x, y] is { } currentnode))
                    {
                        continue;
                    }

                    GetParent((x, y), brushFire, brushFireNodes, brushFire.Size);
                }
            }
            _brushFirecache.Set(brushFire.Origin, brushFireNodes, DateTimeOffset.Now.AddSeconds(10));
            return brushFireNodes;
        }

        private bool IsInBoundaries((short X, short Y) cell, BrushFireNode?[,]? brushFireOut)
        {
            return !(cell.X >= brushFireOut?.GetLength(0) || cell.Y >= brushFireOut?.GetLength(1) ||
                     cell.X < 0 || cell.Y < 0);
        }

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            List<(short X, short Y)> list = new List<(short X, short Y)>();
            _brushFirecache.TryGetValue(end, out BrushFireNode?[,]? brushFireOut);

            if (!IsInBoundaries(start, brushFireOut) || !IsInBoundaries(end, brushFireOut))
            {
                return list;
            }

            if (brushFireOut == null)
            {
                var brushFire = _mapGrid.LoadBrushFire(start, _heuristic);
                brushFireOut = CacheBrushFire(brushFire);
            }

            if (!(brushFireOut[end.X, end.Y] is { } currentnode)) return list;
            while (currentnode.Parent != null)
            {
                list.Add(currentnode.Parent.Position);
                currentnode = currentnode.Parent;
            }

            list.Reverse();
            return list;
        }
    }
}
