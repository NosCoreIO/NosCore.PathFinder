//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Interfaces;
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Benchmark
{
    [MemoryDiagnoser]
    public class PathfinderBenchmarks
    {
        private const short Size = 200;
        private BenchmarkMap _map = null!;
        private JumpPointSearchPathfinder _jps = null!;
        private GoalBasedPathfinder _goalBased = null!;
        private readonly OctileDistanceHeuristic _heuristic = new();

        private readonly (short X, short Y) _start = (2, 2);
        private readonly (short X, short Y) _farGoal = (Size - 3, Size - 3);
        private readonly (short X, short Y) _nearGoal = (14, 12);
        // A walkable cell sealed inside a ring of walls: it can never be reached.
        private readonly (short X, short Y) _unreachableGoal = (Size / 2, Size / 2);

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random(42);
            var cells = new byte[Size, Size];
            for (var x = 0; x < Size; x++)
            {
                for (var y = 0; y < Size; y++)
                {
                    var border = x == 0 || y == 0 || x == Size - 1 || y == Size - 1;
                    cells[x, y] = (byte)(border || random.NextDouble() < 0.2 ? 1 : 0);
                }
            }

            foreach (var (x, y) in new[] { _start, _farGoal, _nearGoal })
            {
                cells[x, y] = 0;
            }

            for (var dx = -2; dx <= 2; dx++)
            {
                for (var dy = -2; dy <= 2; dy++)
                {
                    var ring = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    cells[_unreachableGoal.X + dx, _unreachableGoal.Y + dy] = (byte)(ring == 2 ? 1 : 0);
                }
            }

            _map = new BenchmarkMap(cells);
            _jps = new JumpPointSearchPathfinder(_map, _heuristic);
            _goalBased = new GoalBasedPathfinder(_map, _heuristic);
        }

        [Benchmark]
        public int JumpPointSearch_Far()
        {
            JumpPointSearchPathfinder.Cache.Compact(1);
            return _jps.FindPath(_start, _farGoal).Count();
        }

        [Benchmark]
        public int JumpPointSearch_Near()
        {
            JumpPointSearchPathfinder.Cache.Compact(1);
            return _jps.FindPath(_start, _nearGoal).Count();
        }

        [Benchmark]
        public int JumpPointSearch_Unreachable()
        {
            return _jps.FindPath(_start, _unreachableGoal).Count();
        }

        [Benchmark]
        public int GoalBased_Near()
        {
            return _goalBased.FindPath(_start, _nearGoal).Count();
        }

        [Benchmark]
        public int GoalBased_Unreachable()
        {
            return _goalBased.FindPath(_start, _unreachableGoal).Count();
        }

        [Benchmark]
        public int BrushFire()
        {
            return _map.LoadBrushFire(_nearGoal, _heuristic).Distances.Count;
        }
    }

    public sealed class BenchmarkMap : IMapGrid
    {
        private readonly byte[,] _cells;

        public BenchmarkMap(byte[,] cells)
        {
            _cells = cells;
            Width = (short)cells.GetLength(0);
            Height = (short)cells.GetLength(1);
        }

        public short Width { get; }

        public short Height { get; }

        public byte this[short x, short y] => _cells[x, y];

        public bool IsWalkable(short currentX, short currentY)
        {
            return currentX >= 0 && currentY >= 0 && currentX < Width && currentY < Height && _cells[currentX, currentY] == 0;
        }
    }
}
