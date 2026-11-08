//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
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
            BrushFirecache.Set((mapGrid, brushfire.Origin), brushfire, TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// The cells to walk from <paramref name="start"/> (excluded) to <paramref name="end"/> (included), following the
        /// brushfire around the goal. Empty when start equals end, when either is outside the map or unwalkable, when no
        /// path links them, or when the start is beyond the brushfire's reach.
        /// </summary>
        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            var list = new List<(short X, short Y)>();

            if (start == end || !_mapGrid.IsReachable(start, end))
            {
                return list;
            }

            if (!BrushFirecache.TryGetValue((_mapGrid, end), out BrushFire brushFire))
            {
                brushFire = _mapGrid.LoadBrushFire(end, _heuristic);
                BrushFirecache.Set((_mapGrid, end), brushFire, TimeSpan.FromSeconds(10));
            }

            if (!brushFire.Distances.TryGetValue(start, out var currentDistance))
            {
                return list;
            }

            // Brushfire distances are exact, so each step strictly lowers the distance and the walk cannot loop.
            var current = start;
            while (current != end)
            {
                var bestNeighbor = current;
                var bestDistance = currentDistance;

                for (var direction = 0; direction < GridMoves.Count; direction++)
                {
                    var dx = GridMoves.Dx[direction];
                    var dy = GridMoves.Dy[direction];
                    if (!GridMoves.CanStep(_mapGrid, current.X, current.Y, dx, dy))
                    {
                        continue;
                    }

                    (short X, short Y) neighbor = ((short)(current.X + dx), (short)(current.Y + dy));
                    if (neighbor == end)
                    {
                        bestNeighbor = neighbor;
                        bestDistance = 0;
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
                currentDistance = bestDistance;
                list.Add(current);
            }

            return list;
        }
    }
}
