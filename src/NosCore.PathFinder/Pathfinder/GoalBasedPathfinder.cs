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

        private BrushFire CacheBrushFire(BrushFire brushFire, (short X, short Y) start)
        {
            Node? GetParent((short X, short Y) currentnode)
            {
                var neighbor = _mapGrid.GetNeighbors(currentnode).Select(s => new Node((s.X, s.Y), brushFire.Grid[(s.X, s.Y)]?.Value ?? 0)).OrderBy(s => s.Value).FirstOrDefault();
                if (!(neighbor is { } neighborCell))
                {
                    return null;
                }

                brushFire.Grid[(neighborCell.Position.X, neighborCell.Position.Y)] ??= new Node(neighborCell.Position, null);

                if (neighborCell.Value > 0 && brushFire.Grid[(neighborCell.Position.X, neighborCell.Position.Y)]!.Closed == false)
                {
                    var parent = GetParent(neighborCell.Position);
                    if (parent != null)
                    {
                        brushFire.Grid[(neighborCell.Position.X, neighborCell.Position.Y)]!.Parent ??= parent;
                        brushFire.Grid[(parent.Position.X, parent.Position.Y)] = parent;
                    }
                }

                brushFire.Grid[(neighborCell.Position.X, neighborCell.Position.Y)]!.Closed = true;
                return brushFire.Grid[(neighborCell.Position.X, neighborCell.Position.Y)];

            }

            brushFire.Grid[(start.X, start.Y)] = new Node((start.X, start.Y), brushFire.Grid[(start.X, start.Y)]?.Value ?? 0) { Parent = GetParent((start.X, start.Y)), Closed = true };
            BrushFirecache.Set(brushFire.Origin, brushFire, DateTimeOffset.Now.AddSeconds(10));
            return brushFire;
        }

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            List<(short X, short Y)> list = new List<(short X, short Y)>();
            BrushFirecache.TryGetValue(end, out BrushFire? brushFireOut);

            if (!_mapGrid.IsWalkable(start.X, start.Y) || !_mapGrid.IsWalkable(end.X, end.Y))
            {
                return list;
            }

            if (brushFireOut?.Grid[(start.X, start.Y)]?.Parent == null)
            {
                var brushFire = brushFireOut ?? _mapGrid.LoadBrushFire(end, _heuristic);
                brushFireOut = CacheBrushFire(brushFire, start);
            }

            if (!(brushFireOut?.Grid[(start.X, start.Y)] is { } currentnode)) return list;
            while (currentnode.Parent != null && currentnode.Parent.Position != (start))
            {
                list.Add(currentnode.Parent.Position);
                currentnode = (Node)currentnode.Parent;
            }

            return list;
        }
    }
}
