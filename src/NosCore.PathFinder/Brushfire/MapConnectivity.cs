//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using Microsoft.Extensions.Caching.Memory;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Brushfire
{
    /// <summary>
    /// Connected areas of a grid, labelled once with a flood fill so that an unreachable goal is rejected
    /// in constant time instead of by searching the whole area around the start.
    /// Cached per grid for the same ten seconds as the other pathfinding caches.
    /// </summary>
    internal sealed class MapConnectivity
    {
        private static readonly MemoryCache Cache = new(new MemoryCacheOptions());
        private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(10);

        private readonly int[] _labels;
        private readonly int _width;
        private readonly int _height;

        private MapConnectivity(IMapGrid grid)
        {
            _width = grid.Width;
            _height = grid.Height;
            _labels = new int[_width * _height];
            var queue = new int[_labels.Length];
            var label = 0;

            for (var origin = 0; origin < _labels.Length; origin++)
            {
                if (_labels[origin] != 0 || !grid.IsWalkable((short)(origin % _width), (short)(origin / _width)))
                {
                    continue;
                }

                label++;
                _labels[origin] = label;
                var head = 0;
                var tail = 0;
                queue[tail++] = origin;
                while (head < tail)
                {
                    var cell = queue[head++];
                    var x = cell % _width;
                    var y = cell / _width;
                    for (var direction = 0; direction < GridMoves.Count; direction++)
                    {
                        var dx = GridMoves.Dx[direction];
                        var dy = GridMoves.Dy[direction];
                        var next = cell + dy * _width + dx;
                        if ((uint)(x + dx) < (uint)_width && (uint)(y + dy) < (uint)_height && _labels[next] == 0 &&
                            GridMoves.CanStep(grid, x, y, dx, dy))
                        {
                            _labels[next] = label;
                            queue[tail++] = next;
                        }
                    }
                }
            }
        }

        public static MapConnectivity For(IMapGrid grid)
        {
            if (Cache.TryGetValue(grid, out MapConnectivity? connectivity) && connectivity != null &&
                connectivity._width == grid.Width && connectivity._height == grid.Height)
            {
                return connectivity;
            }

            connectivity = new MapConnectivity(grid);
            Cache.Set(grid, connectivity, Lifetime);
            return connectivity;
        }

        public bool AreConnected(int startX, int startY, int endX, int endY)
        {
            if ((uint)startX >= (uint)_width || (uint)startY >= (uint)_height ||
                (uint)endX >= (uint)_width || (uint)endY >= (uint)_height)
            {
                return false;
            }

            var label = _labels[startY * _width + startX];
            return label != 0 && label == _labels[endY * _width + endX];
        }
    }
}
