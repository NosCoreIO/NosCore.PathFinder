//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

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

        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
#pragma warning disable 618
            var linearPathfinder = new LinearPathfinder(_mapGrid, _heuristic);
#pragma warning restore 618
            var path = new List<(short X, short Y)>();
            var jumps = GetJumpList(start, end).ToList();
            for (var i = 0; i < jumps.Count - 1; i++)
            {
                path.AddRange(linearPathfinder.FindPath(jumps[i], jumps[i + 1]));
            }

            path.RemoveAt(path.Count - 1);
            return path;
        }

        internal IEnumerable<(short X, short Y)> GetJumpList((short X, short Y) start, (short X, short Y) end)
        {
            var Nodes = new JumpNode?[_mapGrid.XLength, _mapGrid.YLength];
            var startNode = new JumpNode(start, _mapGrid[start.X, start.Y]) { F = 0, G = 0, Opened = true };
            Nodes[start.X, start.Y] = startNode;
            var heap = new MinHeap();
            heap.Push(startNode);

            while (heap.Count != 0)
            {
                var node = heap.Pop();

                if (node.Position == end)
                {
                    return Trace(node);
                }

                IdentitySuccessors((JumpNode)node, Nodes, end, heap);
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

        private void IdentitySuccessors(JumpNode node, JumpNode?[,] Nodes, (short X, short Y) end, MinHeap heap)
        {
            foreach (var neighbour in _mapGrid.GetNeighbors(node.Position))
            {
                var jumpPoint = Jump(neighbour, node.Position, end);
                if (jumpPoint == null)
                {
                    continue;
                }

                var jumpNode = Nodes[jumpPoint.Value.X, jumpPoint.Value.Y] ??
                               new JumpNode((jumpPoint.Value.X, jumpPoint.Value.Y),
                                   _mapGrid[jumpPoint.Value.X, jumpPoint.Value.Y]);
                Nodes[jumpPoint.Value.X, jumpPoint.Value.Y] = jumpNode;

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
                else
                {
                    heap.Push(jumpNode);
                }
            }
        }

        public (short X, short Y)? Jump((short X, short Y) current, (short X, short Y) proposed, (short X, short Y) end)
        {
            var x = current.X;
            var y = current.Y;
            var dx = current.X - proposed.X;
            var dy = current.Y - proposed.Y;

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
