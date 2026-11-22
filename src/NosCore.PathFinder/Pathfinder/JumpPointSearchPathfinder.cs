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
    public class JumpPointSearchPathfinder : IPathfinder
    {
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(10);

        [ThreadStatic]
        private static SearchState? _searchState;

        private readonly IMapGrid _mapGrid;
        public static readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IHeuristic _heuristic;

        public JumpPointSearchPathfinder(IMapGrid mapGrid, IHeuristic heuristic)
        {
            _mapGrid = mapGrid;
            _heuristic = heuristic;
        }

        private static (short X, short Y)[] BuildFullPath(List<(short X, short Y)> routeFound)
        {
            var length = 0;
            for (var i = 0; i < routeFound.Count - 1; i++)
            {
                length += Math.Max(Math.Abs(routeFound[i + 1].X - routeFound[i].X), Math.Abs(routeFound[i + 1].Y - routeFound[i].Y));
            }

            var path = new (short X, short Y)[length];
            var index = 0;
            for (var routeTrav = 0; routeTrav < routeFound.Count - 1; routeTrav++)
            {
                var fromGrid = routeFound[routeTrav];
                var toGrid = routeFound[routeTrav + 1];
                var nDx = (short)Math.Sign(toGrid.X - fromGrid.X);
                var nDy = (short)Math.Sign(toGrid.Y - fromGrid.Y);

                while (fromGrid.X != toGrid.X || fromGrid.Y != toGrid.Y)
                {
                    fromGrid.X += nDx;
                    fromGrid.Y += nDy;
                    path[index++] = fromGrid;
                }
            }

            return path;
        }

        /// <summary>
        /// The cells to walk from <paramref name="start"/> (excluded) to <paramref name="end"/> (included).
        /// Empty when start equals end, when either is outside the map or unwalkable, or when no path links them;
        /// the last case is answered from cached connectivity data without searching.
        /// </summary>
        public IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end)
        {
            if (start == end || !_mapGrid.IsReachable(start, end))
            {
                return Array.Empty<(short X, short Y)>();
            }

            // Keyed by grid too: the cache is shared by every pathfinder instance, one per map.
            if (Cache.TryGetValue((_mapGrid, start, end), out IEnumerable<(short X, short Y)>? cachedList))
            {
                return cachedList ?? Array.Empty<(short X, short Y)>();
            }

            var path = BuildFullPath(GetJumpList(start, end));
            for (var i = 0; i < path.Length; i++)
            {
                Cache.Set((_mapGrid, path[i], end), (IEnumerable<(short X, short Y)>)new ArraySegment<(short X, short Y)>(path, i + 1, path.Length - i - 1), CacheLifetime);
            }
            return path;
        }

        internal List<(short X, short Y)> GetJumpList((short X, short Y) start, (short X, short Y) end)
        {
            var result = new List<(short X, short Y)>();
            if (!GridMoves.IsWalkable(_mapGrid, start.X, start.Y) || !GridMoves.IsWalkable(_mapGrid, end.X, end.Y))
            {
                return result;
            }

            int width = _mapGrid.Width;
            var state = _searchState ??= new SearchState();
            state.Begin(width * _mapGrid.Height);

            var startIndex = start.Y * width + start.X;
            var endIndex = end.Y * width + end.X;
            state.Open(startIndex, 0, -1);
            state.Heap.Enqueue(startIndex, 0);

            while (state.Heap.TryDequeue(out var index, out _))
            {
                if (state.IsClosed(index))
                {
                    continue;
                }

                state.Close(index);
                if (index == endIndex)
                {
                    for (var current = index; current != -1; current = state.Parent[current])
                    {
                        result.Add(((short)(current % width), (short)(current / width)));
                    }

                    result.Reverse();
                    return result;
                }

                IdentifySuccessors(state, index % width, index / width, end);
            }

            return result;
        }

        private void IdentifySuccessors(SearchState state, int x, int y, (short X, short Y) end)
        {
            int width = _mapGrid.Width;
            var index = y * width + x;
            for (var direction = 0; direction < GridMoves.Count; direction++)
            {
                var dx = GridMoves.Dx[direction];
                var dy = GridMoves.Dy[direction];
                if (!GridMoves.CanStep(_mapGrid, x, y, dx, dy) ||
                    !JumpFrom(x + dx, y + dy, dx, dy, end.X, end.Y, out var jumpX, out var jumpY))
                {
                    continue;
                }

                var jumpIndex = jumpY * width + jumpX;
                if (state.IsClosed(jumpIndex))
                {
                    continue;
                }

                var g = state.G[index] + _heuristic.GetDistance(((short)jumpX, (short)jumpY), ((short)x, (short)y));
                if (state.IsOpened(jumpIndex) && !(g < state.G[jumpIndex]))
                {
                    continue;
                }

                state.Open(jumpIndex, g, index);
                // No decrease-key in PriorityQueue: enqueue again, the stale entry is skipped once closed.
                state.Heap.Enqueue(jumpIndex, g + _heuristic.GetDistance(((short)jumpX, (short)jumpY), end));
            }
        }

        public (short X, short Y)? Jump((short X, short Y) current, (short X, short Y) proposed, (short X, short Y) end)
        {
            return JumpFrom(current.X, current.Y, current.X - proposed.X, current.Y - proposed.Y, end.X, end.Y, out var x, out var y)
                ? ((short)x, (short)y)
                : null;
        }

        // Walks from (x, y) in direction (dx, dy) until the goal, a jump point, or a dead end.
        private bool JumpFrom(int x, int y, int dx, int dy, int endX, int endY, out int jumpX, out int jumpY)
        {
            while (true)
            {
                jumpX = x;
                jumpY = y;
                if (!Walkable(x, y))
                {
                    return false;
                }

                if (x == endX && y == endY)
                {
                    return true;
                }

                if (dx != 0 && dy != 0)
                {
                    if ((Walkable(x - dx, y + dy) && !Walkable(x - dx, y)) ||
                        (Walkable(x + dx, y - dy) && !Walkable(x, y - dy)))
                    {
                        return true;
                    }

                    if (JumpFrom(x + dx, y, dx, 0, endX, endY, out _, out _) ||
                        JumpFrom(x, y + dy, 0, dy, endX, endY, out _, out _))
                    {
                        jumpX = x;
                        jumpY = y;
                        return true;
                    }

                    if (!Walkable(x + dx, y) && !Walkable(x, y + dy))
                    {
                        return false;
                    }
                }
                else if (dx != 0)
                {
                    if ((Walkable(x + dx, y + 1) && !Walkable(x, y + 1)) ||
                        (Walkable(x + dx, y - 1) && !Walkable(x, y - 1)))
                    {
                        return true;
                    }
                }
                else if (dy != 0)
                {
                    if ((Walkable(x + 1, y + dy) && !Walkable(x + 1, y)) ||
                        (Walkable(x - 1, y + dy) && !Walkable(x - 1, y)))
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }

                x += dx;
                y += dy;
            }
        }

        private bool Walkable(int x, int y) => GridMoves.IsWalkable(_mapGrid, x, y);

        // Per-thread search buffers reused across searches; a generation stamp replaces clearing them.
        private sealed class SearchState
        {
            private int[] _openedAt = Array.Empty<int>();
            private int[] _closedAt = Array.Empty<int>();
            private int _generation;

            public double[] G { get; private set; } = Array.Empty<double>();

            public int[] Parent { get; private set; } = Array.Empty<int>();

            public PriorityQueue<int, double> Heap { get; } = new();

            public void Begin(int cells)
            {
                Heap.Clear();
                if (_openedAt.Length < cells)
                {
                    _openedAt = new int[cells];
                    _closedAt = new int[cells];
                    G = new double[cells];
                    Parent = new int[cells];
                    _generation = 0;
                }

                if (_generation == int.MaxValue)
                {
                    Array.Clear(_openedAt);
                    Array.Clear(_closedAt);
                    _generation = 0;
                }

                _generation++;
            }

            public bool IsOpened(int index) => _openedAt[index] == _generation;

            public bool IsClosed(int index) => _closedAt[index] == _generation;

            public void Open(int index, double g, int parent)
            {
                _openedAt[index] = _generation;
                G[index] = g;
                Parent[index] = parent;
            }

            public void Close(int index) => _closedAt[index] = _generation;
        }
    }
}
