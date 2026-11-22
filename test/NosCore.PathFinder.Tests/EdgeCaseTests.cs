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
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class EdgeCaseTests
    {
        private static readonly OctileDistanceHeuristic Heuristic = new();

        // Left room and right room split by a full wall at column 2.
        private static TestMap SplitMap() => new(
        [
            [0, 0, 1, 0, 0],
            [0, 0, 1, 0, 0],
            [0, 0, 1, 0, 0],
        ]);

        private static TestMap OpenMap() => new(
        [
            [0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0],
        ]);

        [TestMethod]
        public void GoalBasedFindsNothingFromAWall()
        {
            var map = SplitMap();

            Assert.IsFalse(new GoalBasedPathfinder(map, Heuristic).FindPath((2, 0), (0, 0)).Any());
            Assert.IsFalse(new GoalBasedPathfinder(map, Heuristic).FindPath((0, 0), (2, 0)).Any());
        }

        [TestMethod]
        public void GoalBasedFindsNothingAcrossAFullWall()
        {
            var map = SplitMap();

            Assert.IsFalse(new GoalBasedPathfinder(map, Heuristic).FindPath((0, 1), (4, 1)).Any());
        }

        [TestMethod]
        public void GoalBasedWalksOneCellAtATimeToTheGoal()
        {
            var map = OpenMap();

            var path = new GoalBasedPathfinder(map, Heuristic).FindPath((0, 0), (4, 2)).ToList();

            Assert.AreEqual(((short)4, (short)2), path[^1]);
            AssertAdjacentSteps((0, 0), path);
        }

        [TestMethod]
        public void GoalBasedUsesAGivenBrushFire()
        {
            var map = OpenMap();
            var empty = new BrushFire((4, 2), new Dictionary<(short X, short Y), double>(), map.Width, map.Height);

            var path = new GoalBasedPathfinder(map, Heuristic, empty).FindPath((0, 0), (4, 2));

            Assert.IsFalse(path.Any());
        }

        [TestMethod]
        public void JumpPointFindsNothingFromAWall()
        {
            var map = SplitMap();

            Assert.IsFalse(new JumpPointSearchPathfinder(map, Heuristic).FindPath((2, 0), (0, 0)).Any());
            Assert.IsFalse(new JumpPointSearchPathfinder(map, Heuristic).FindPath((0, 0), (2, 1)).Any());
        }

        [TestMethod]
        public void JumpPointFindsNothingAcrossAFullWall()
        {
            var map = SplitMap();

            Assert.IsFalse(new JumpPointSearchPathfinder(map, Heuristic).FindPath((0, 2), (4, 0)).Any());
        }

        [TestMethod]
        public void JumpPointWalksOneCellAtATimeToTheGoal()
        {
            var map = OpenMap();

            var path = new JumpPointSearchPathfinder(map, Heuristic).FindPath((0, 0), (4, 2)).ToList();

            Assert.AreEqual(((short)4, (short)2), path[^1]);
            AssertAdjacentSteps((0, 0), path);
        }

        [TestMethod]
        public void ABrushFireOutsideTheMapReachesNothing()
        {
            var map = OpenMap();

            Assert.AreEqual(0, map.LoadBrushFire((-1, 0), Heuristic).Distances.Count);
            Assert.AreEqual(0, map.LoadBrushFire((0, 3), Heuristic).Distances.Count);
            Assert.AreEqual(0, map.LoadBrushFire((5, 0), Heuristic).Distances.Count);
        }

        [TestMethod]
        public void ABrushFireStopsAtItsMaximumDistance()
        {
            var map = OpenMap();

            var brushFire = map.LoadBrushFire((0, 1), Heuristic, 2);

            Assert.AreEqual(2, brushFire[2, 1]);
            Assert.IsNull(brushFire[3, 1]);
        }

        [TestMethod]
        public void ABrushFireDoesNotCrossAFullWall()
        {
            var map = SplitMap();

            var brushFire = map.LoadBrushFire((0, 0), Heuristic);

            Assert.IsNull(brushFire[3, 0]);
            Assert.IsNull(brushFire[2, 0]);
            Assert.AreEqual(0, brushFire[0, 0]);
        }

        [TestMethod]
        public void TheFlowFieldHasNoVectorAtItsOriginOrWithinTheStopDistance()
        {
            var map = OpenMap();
            var flowField = map.LoadBrushFire((0, 1), Heuristic).GetFlowField(map, 1);

            Assert.IsNull(flowField[0, 1]);
            Assert.IsNull(flowField[1, 1]);
            Assert.IsNotNull(flowField[3, 1]);
        }

        [TestMethod]
        public void ACornerHasThreeNeighbours()
        {
            var map = OpenMap();

            Assert.AreEqual(3, map.GetNeighbors((0, 0)).Count());
            Assert.AreEqual(8, map.GetNeighbors((2, 1)).Count());
        }

        [TestMethod]
        public void WallsAreNeighboursOnlyWhenAskedFor()
        {
            var map = SplitMap();

            Assert.IsFalse(map.GetNeighbors((1, 1)).Any(n => n.X == 2));
            Assert.AreEqual(3, map.GetNeighbors((1, 1), true).Count(n => n.X == 2));
        }

        private static void AssertAdjacentSteps((short X, short Y) start, List<(short X, short Y)> path)
        {
            var previous = start;
            foreach (var step in path)
            {
                Assert.IsTrue(Math.Abs(step.X - previous.X) <= 1 && Math.Abs(step.Y - previous.Y) <= 1,
                    $"{previous} to {step} is not one step");
                previous = step;
            }
        }
    }
}
