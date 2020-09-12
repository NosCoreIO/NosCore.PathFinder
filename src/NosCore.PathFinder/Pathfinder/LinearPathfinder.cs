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
    [Obsolete("This pathfinder should not be used it won't find the target if a wall is in between. It should only be used when some other system remove walls in between")]
    internal class LinearPathfinder : IPathfinder
    {
        private readonly IMapGrid _mapGrid;
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHeuristic _heuristic;

        public LinearPathfinder(IMapGrid mapGrid, IHeuristic heuristic)
        {
            _mapGrid = mapGrid;
            _heuristic = heuristic;
        }


        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            var list = new List<(short X, short Y)>();
            var currentNode = start;
            do
            {
                if (Cache.TryGetValue((start, end), out IEnumerable<(short X, short Y)> cachedList))
                {
                    return cachedList;
                }
                currentNode = _mapGrid.GetNeighbors(currentNode, true).OrderBy(s => _heuristic.GetDistance(s, end))
                .FirstOrDefault();
                if (_mapGrid.IsWalkable(currentNode.X, currentNode.Y))
                {
                    list.Add(currentNode);
                }
            } while (currentNode != end && _mapGrid.IsWalkable(currentNode.X, currentNode.Y));

            for (var i = 0; i < list.Count; i++)
            {
                Cache.Set((list[i], end), list.Skip(i), DateTimeOffset.Now.AddSeconds(10));
            }
            return list;
        }
    }
}
