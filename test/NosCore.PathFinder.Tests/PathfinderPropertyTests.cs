//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Interfaces;
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class PathfinderPropertyTests
    {
        private static readonly double Sqrt2 = Math.Sqrt(2);

        [TestMethod]
        public void JumpPointSearchMatchesAReferenceSearchOnRandomGrids()
        {
            var random = new Random(1234);
            for (var iteration = 0; iteration < 400; iteration++)
            {
                var map = RandomMap(random, random.Next(2, 30), random.Next(2, 30), random.NextDouble() * 0.45);
                var start = RandomCell(random, map);
                var end = RandomCell(random, map);
                var expected = ReferenceCost(map, start, end);

                var path = new JumpPointSearchPathfinder(map, new OctileDistanceHeuristic()).FindPath(start, end).ToList();

                var context = $"iteration {iteration}, {start} -> {end}\n{Draw(map)}";
                if (expected == null || start == end)
                {
                    Assert.IsEmpty(path, context);
                    continue;
                }

                Assert.IsNotEmpty(path, context);
                Assert.AreEqual(end, path.Last(), context);
                var cost = 0d;
                var previous = start;
                foreach (var step in path)
                {
                    Assert.IsTrue(IsLegalStep(map, previous, step), $"illegal step {previous} -> {step}, {context}");
                    cost += previous.X != step.X && previous.Y != step.Y ? Sqrt2 : 1;
                    previous = step;
                }

                Assert.AreEqual(expected.Value, cost, 1e-6, context);
            }
        }

        [TestMethod]
        public void GoalBasedPathfinderOnlyTakesLegalStepsOnRandomGrids()
        {
            var random = new Random(4321);
            for (var iteration = 0; iteration < 200; iteration++)
            {
                var map = RandomMap(random, random.Next(2, 20), random.Next(2, 20), random.NextDouble() * 0.45);
                var start = RandomCell(random, map);
                var end = RandomCell(random, map);
                var expected = ReferenceCost(map, start, end);

                var path = new GoalBasedPathfinder(map, new OctileDistanceHeuristic()).FindPath(start, end).ToList();

                var context = $"iteration {iteration}, {start} -> {end}\n{Draw(map)}";
                if (expected == null || start == end)
                {
                    Assert.IsEmpty(path, context);
                    continue;
                }

                if (expected <= 22)
                {
                    Assert.AreEqual(end, path.Last(), context);
                }

                var previous = start;
                foreach (var step in path)
                {
                    Assert.IsTrue(IsLegalStep(map, previous, step), $"illegal step {previous} -> {step}, {context}");
                    previous = step;
                }
            }
        }

        [TestMethod]
        public void IsReachableAgreesWithAReferenceSearchOnRandomGrids()
        {
            var random = new Random(99);
            for (var iteration = 0; iteration < 300; iteration++)
            {
                var map = RandomMap(random, random.Next(1, 25), random.Next(1, 25), random.NextDouble() * 0.6);
                var start = RandomCell(random, map);
                var end = RandomCell(random, map);

                Assert.AreEqual(ReferenceCost(map, start, end) != null, map.IsReachable(start, end),
                    $"iteration {iteration}, {start} -> {end}\n{Draw(map)}");
            }
        }

        internal static bool IsLegalStep(TestMap map, (short X, short Y) from, (short X, short Y) to)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || (dx == 0 && dy == 0) || !Walkable(map, to.X, to.Y))
            {
                return false;
            }

            return dx == 0 || dy == 0 || Walkable(map, from.X + dx, from.Y) || Walkable(map, from.X, from.Y + dy);
        }

        // Plain Dijkstra over the 8-connected grid: a diagonal step needs one of its two orthogonal cells open.
        private static double? ReferenceCost(TestMap map, (short X, short Y) start, (short X, short Y) end)
        {
            if (!Walkable(map, start.X, start.Y) || !Walkable(map, end.X, end.Y))
            {
                return null;
            }

            var best = new Dictionary<(short X, short Y), double> { [start] = 0 };
            var queue = new PriorityQueue<(short X, short Y), double>();
            queue.Enqueue(start, 0);
            while (queue.TryDequeue(out var cell, out var cost))
            {
                if (cell == end)
                {
                    return cost;
                }

                if (cost > best[cell])
                {
                    continue;
                }

                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        (short X, short Y) next = ((short)(cell.X + dx), (short)(cell.Y + dy));
                        if (!IsLegalStep(map, cell, next))
                        {
                            continue;
                        }

                        var nextCost = cost + (dx != 0 && dy != 0 ? Sqrt2 : 1);
                        if (!best.TryGetValue(next, out var known) || nextCost < known - 1e-9)
                        {
                            best[next] = nextCost;
                            queue.Enqueue(next, nextCost);
                        }
                    }
                }
            }

            return null;
        }

        private static bool Walkable(TestMap map, int x, int y)
        {
            return x >= 0 && y >= 0 && x < map.Width && y < map.Height && map.IsWalkable((short)x, (short)y);
        }

        // No border of walls on purpose: paths along the map edges must not read outside the grid.
        private static TestMap RandomMap(Random random, int width, int height, double wallDensity)
        {
            return new TestMap(Enumerable.Range(0, height)
                .Select(_ => Enumerable.Range(0, width).Select(_ => (byte)(random.NextDouble() < wallDensity ? 1 : 0)).ToArray())
                .ToArray());
        }

        private static (short X, short Y) RandomCell(Random random, TestMap map)
        {
            return ((short)random.Next(map.Width), (short)random.Next(map.Height));
        }

        private static string Draw(TestMap map)
        {
            return string.Join("\n", Enumerable.Range(0, map.Height)
                .Select(y => new string(Enumerable.Range(0, map.Width).Select(x => map.IsWalkable((short)x, (short)y) ? '.' : '#').ToArray())));
        }
    }
}
