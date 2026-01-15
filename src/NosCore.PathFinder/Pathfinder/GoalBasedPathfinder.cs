//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

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
        public static readonly MemoryCache BrushFirecache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHeuristic _heuristic;

        public GoalBasedPathfinder(IMapGrid mapGrid, IHeuristic heuristic)
        {
            _mapGrid = mapGrid;
            _heuristic = heuristic;
        }

        public GoalBasedPathfinder(IMapGrid mapGrid, IHeuristic heuristic, BrushFire brushfire) : this(mapGrid, heuristic)
        {
            BrushFirecache.Set(brushfire.Origin, brushfire, DateTimeOffset.Now.AddSeconds(10));
        }

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            var list = new List<(short X, short Y)>();

            if (!_mapGrid.IsWalkable(start.X, start.Y) || !_mapGrid.IsWalkable(end.X, end.Y))
            {
                return list;
            }

            if (!BrushFirecache.TryGetValue(end, out BrushFire brushFire))
            {
                brushFire = _mapGrid.LoadBrushFire(end, _heuristic);
                BrushFirecache.Set(end, brushFire, DateTimeOffset.Now.AddSeconds(10));
            }

            if (!brushFire.Distances.ContainsKey(start))
            {
                return list;
            }

            var current = start;
            var visited = new HashSet<(short X, short Y)> { current };

            while (current != end)
            {
                var bestNeighbor = current;
                var bestDistance = brushFire.Distances.TryGetValue(current, out var currentDist) ? currentDist : double.MaxValue;

                foreach (var neighbor in _mapGrid.GetNeighbors(current))
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    if (neighbor == end)
                    {
                        bestNeighbor = neighbor;
                        break;
                    }

                    if (brushFire.Distances.TryGetValue(neighbor, out var neighborDist) && neighborDist < bestDistance)
                    {
                        bestDistance = neighborDist;
                        bestNeighbor = neighbor;
                    }
                }

                if (bestNeighbor == current)
                {
                    break;
                }

                current = bestNeighbor;
                visited.Add(current);
                list.Add(current);
            }

            return list;
        }
    }
}
