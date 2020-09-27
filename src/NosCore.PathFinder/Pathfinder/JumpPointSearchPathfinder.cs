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
    public class JumpPointSearchPathfinder : IPathfinder
    {
        private readonly IMapGrid _mapGrid;
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHeuristic _heuristic;

        public JumpPointSearchPathfinder(IMapGrid mapGrid, IHeuristic heuristic)
        {
            _mapGrid = mapGrid;
            _heuristic = heuristic;
        }

        List<(short X, short Y)> BuildFullPath(List<(short X, short Y)> routeFound)
        {
            var path = new List<(short X, short Y)>();
            for (var routeTrav = 0; routeTrav < routeFound.Count - 1; routeTrav++)
            {
                var fromGrid = routeFound[routeTrav];
                var toGrid = routeFound[routeTrav + 1];
                var dX = toGrid.X - fromGrid.X;
                var dY = toGrid.Y - fromGrid.Y;

                var nDx = 0;
                var nDy = 0;
                if (dX != 0)
                {
                    nDx = (dX / Math.Abs(dX));
                }
                if (dY != 0)
                {
                    nDy = (dY / Math.Abs(dY));
                }

                while (fromGrid.X != toGrid.X || fromGrid.Y != toGrid.Y)
                {
                    fromGrid.X += (short)nDx;
                    fromGrid.Y += (short)nDy;
                    path.Add(fromGrid);
                }
            }

            return path;
        }

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            if (Cache.TryGetValue((start, end), out IEnumerable<(short X, short Y)> cachedList))
            {
                return cachedList;
            }

            var path = BuildFullPath(GetJumpList(start, end).ToList());
            for (var i = 0; i < path.Count; i++)
            {
                Cache.Set((path[i], end), path.Skip(i), DateTimeOffset.Now.AddSeconds(10));
            }
            return path;
        }

        internal IEnumerable<(short X, short Y)> GetJumpList((short X, short Y) start, (short X, short Y) end)
        {
            var nodes = new JumpNode?[_mapGrid.Width, _mapGrid.Length];
            if (_mapGrid.IsWalkable(start.X, start.Y) && _mapGrid.IsWalkable(end.X, end.Y))
            {
                var startNode = new JumpNode(start, _mapGrid[start.X, start.Y]) { F = 0, G = 0, Opened = true };
                nodes[start.X, start.Y] = startNode;
                var heap = new MinHeap();
                heap.Push(startNode);

                while (heap.Count != 0)
                {
                    var node = heap.Pop();
                    node.Closed = true;
                    if (node.Position == end)
                    {
                        return Trace(node);
                    }

                    IdentifySuccessors((JumpNode)node, nodes, end, heap);
                }
            }
            return new (short X, short Y)[0];
        }

        private List<(short X, short Y)> Trace(Node node)
        {
            var path = new List<Node> { node };
            while (node.Parent != null)
            {
                node = node.Parent;
                path.Add(node);
            }
            path.Reverse();
            return path.Select(s => s.Position).ToList();
        }

        private void IdentifySuccessors(JumpNode node, JumpNode?[,] nodes, (short X, short Y) end, MinHeap heap)
        {
            foreach (var neighbour in _mapGrid.GetNeighbors(node.Position))
            {
                var jumpPoint = Jump(neighbour, node.Position, end);
                if (jumpPoint == null)
                {
                    continue;
                }

                var jumpNode = nodes[jumpPoint.Value.X, jumpPoint.Value.Y] ??
                               new JumpNode((jumpPoint.Value.X, jumpPoint.Value.Y),
                                   _mapGrid[jumpPoint.Value.X, jumpPoint.Value.Y]);
                nodes[jumpPoint.Value.X, jumpPoint.Value.Y] = jumpNode;

                if (jumpNode.Closed)
                {
                    continue;
                }

                var d = _heuristic.GetDistance(((short X, short Y))jumpPoint, node.Position);
                var ng = node.G + d;

                if (jumpNode.Opened && !(ng < jumpNode.G))
                {
                    continue;
                }
                jumpNode.G = ng;
                jumpNode.H ??= _heuristic.GetDistance(jumpPoint.Value, end);
                jumpNode.F = jumpNode.G + jumpNode.H.Value;
                jumpNode.Parent = node;

                if (!jumpNode.Opened)
                {
                    heap.Push(jumpNode);
                    jumpNode.Opened = true;
                }
            }
        }

        public (short X, short Y)? Jump((short X, short Y) current, (short X, short Y) proposed, (short X, short Y) end)
        {
            var x = current.X;
            var y = current.Y;
            var dx = x - proposed.X;
            var dy = y - proposed.Y;

            if (!_mapGrid.IsWalkable(x, y))
            {
                return null;
            }

            if (end == current)
            {
                return current;
            }

            if (dx != 0 && dy != 0)
            {
                if ((_mapGrid.IsWalkable((short)(x - dx), (short)(y + dy)) &&
                     !_mapGrid.IsWalkable((short)(x - dx), y)) ||
                    (_mapGrid.IsWalkable((short)(x + dx), (short)(y - dy)) &&
                     !_mapGrid.IsWalkable(x, (short)(y - dy))))
                {
                    return current;
                }

                if (Jump(((short)(x + dx), y), current, end) != null ||
                    Jump((x, (short)(y + dy)), current, end) != null)
                {
                    return current;
                }
            }
            else
            {
                if (dx != 0)
                {
                    if ((_mapGrid.IsWalkable((short)(x + dx), (short)(y + 1)) && !_mapGrid.IsWalkable(x, (short)(y + 1))) ||
                        (_mapGrid.IsWalkable((short)(x + dx), (short)(y - 1)) && !_mapGrid.IsWalkable(x, (short)(y - 1))))
                    {
                        return current;
                    }
                }

                if ((_mapGrid.IsWalkable((short)(x + 1), (short)(y + dy)) &&
                     !_mapGrid.IsWalkable((short)(x + 1), y)) ||
                    (_mapGrid.IsWalkable((short)(x - 1), (short)(y + dy)) &&
                     !_mapGrid.IsWalkable((short)(x - 1), y)))
                {
                    return current;
                }
            }

            if (_mapGrid.IsWalkable((short)(x + dx), y) || _mapGrid.IsWalkable(x, (short)(y + dy)))
            {
                return Jump(((short)(x + dx), (short)(y + dy)), current, end);
            }

            return null;
        }
    }
}
