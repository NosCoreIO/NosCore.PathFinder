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

   

        private Node?[,] CacheBrushFire(BrushFire brushFire)
        {
            var Nodes = new Node?[brushFire.Width, brushFire.Length];
            Node? GetParent((short X, short Y) currentnode)
            {
                var newnode = _mapGrid.GetNeighbors(currentnode).Select(s => new Node((s.X, s.Y), brushFire[s.X, s.Y] ?? 0)).OrderBy(s => s.Value).FirstOrDefault();
                if (!(newnode is { } cell))
                {
                    return null;
                }

                var currentNode = new Node(cell.Position, null);
                Nodes[cell.Position.X, cell.Position.Y] ??= currentNode;

                if (cell.Value > 1 && Nodes[cell.Position.X, cell.Position.Y]!.Closed == false)
                {
                    currentNode.Parent ??= GetParent(cell.Position);
                }

                Nodes[cell.Position.X, cell.Position.Y]!.Closed = true;
                return Nodes[cell.Position.X, cell.Position.Y];

            }
            for (short y = 0; y < brushFire.Width; y++)
            {
                for (short x = 0; x < brushFire.Length; x++)
                {
                    if (!(brushFire[x, y] is { } || Nodes[x, y]?.Closed == true))
                    {
                        continue;
                    }

                    GetParent((x, y));
                }
            }

            BrushFirecache.Set(brushFire.Origin, Nodes, DateTimeOffset.Now.AddSeconds(10));
            return Nodes;
        }

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            List<(short X, short Y)> list = new List<(short X, short Y)>();
            BrushFirecache.TryGetValue(end, out Node?[,]? brushFireOut);

            if (!_mapGrid.IsWalkable(start.X, start.Y) || !_mapGrid.IsWalkable(end.X, end.Y))
            {
                return list;
            }

            if (brushFireOut == null)
            {
                var brushFire = _mapGrid.LoadBrushFire(end, _heuristic);
                brushFireOut = CacheBrushFire(brushFire);
            }

            if (!(brushFireOut[start.X, start.Y] is { } currentnode)) return list;
            while (currentnode.Parent != null && currentnode.Parent.Position != (start))
            {
                list.Add(currentnode.Parent.Position);
                currentnode = (Node)currentnode.Parent;
            }

            return list;
        }
    }
}
