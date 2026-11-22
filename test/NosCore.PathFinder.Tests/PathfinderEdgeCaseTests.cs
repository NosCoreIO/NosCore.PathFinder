//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

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
    public class PathfinderEdgeCaseTests
    {
        private static readonly TestMap Open = new(new[]
        {
            new byte[] { 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0 },
            new byte[] { 0, 0, 0, 0 },
        });

        // The only way from the top-left to the bottom-right is squeezing between two walls that touch by a corner.
        private static readonly TestMap Squeeze = new(new[]
        {
            new byte[] { 0, 1 },
            new byte[] { 1, 0 },
        });

        private static IEnumerable<IPathfinder> Pathfinders(IMapGrid map)
        {
            yield return new JumpPointSearchPathfinder(map, new OctileDistanceHeuristic());
            yield return new GoalBasedPathfinder(map, new OctileDistanceHeuristic());
        }

        [TestMethod]
        [DataRow((short)-1, (short)0, (short)3, (short)2)]
        [DataRow((short)0, (short)0, (short)4, (short)2)]
        [DataRow((short)0, (short)0, (short)3, (short)-5)]
        [DataRow((short)9, (short)9, (short)0, (short)0)]
        public void AStartOrGoalOutsideTheMapHasNoPath(short startX, short startY, short endX, short endY)
        {
            foreach (var pathfinder in Pathfinders(Open))
            {
                Assert.IsEmpty(pathfinder.FindPath((startX, startY), (endX, endY)).ToList(), pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void AnUnwalkableStartOrGoalHasNoPath()
        {
            foreach (var pathfinder in Pathfinders(Squeeze))
            {
                Assert.IsEmpty(pathfinder.FindPath((1, 0), (1, 1)).ToList(), pathfinder.GetType().Name);
                Assert.IsEmpty(pathfinder.FindPath((0, 0), (0, 1)).ToList(), pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void StartingOnTheGoalIsAnEmptyPath()
        {
            foreach (var pathfinder in Pathfinders(Open))
            {
                Assert.IsEmpty(pathfinder.FindPath((1, 1), (1, 1)).ToList(), pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void PathsAlongTheMapBorderStayInsideTheMap()
        {
            foreach (var pathfinder in Pathfinders(Open))
            {
                var path = pathfinder.FindPath((0, 0), (3, 2)).ToList();

                Assert.AreEqual(((short)3, (short)2), path.Last(), pathfinder.GetType().Name);
                Assert.AreEqual(3, path.Count, pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void NoPathfinderSqueezesBetweenTwoWallsTouchingByACorner()
        {
            foreach (var pathfinder in Pathfinders(Squeeze))
            {
                Assert.IsEmpty(pathfinder.FindPath((0, 0), (1, 1)).ToList(), pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void NeighborsDoNotSqueezeBetweenTwoWallsTouchingByACorner()
        {
            Assert.IsEmpty(Squeeze.GetNeighbors((0, 0)).ToList());
            Assert.HasCount(3, Squeeze.GetNeighbors((0, 0), includeWalls: true).ToList());
        }

        [TestMethod]
        public void TheBrushFireDoesNotLeakBetweenTwoWallsTouchingByACorner()
        {
            var brushFire = Squeeze.LoadBrushFire((0, 0), new OctileDistanceHeuristic());

            Assert.IsNull(brushFire[1, 1]);
        }

        [TestMethod]
        public void CuttingAroundASingleWallCornerIsAllowed()
        {
            var map = new TestMap(new[]
            {
                new byte[] { 0, 1 },
                new byte[] { 0, 0 },
            });

            foreach (var pathfinder in Pathfinders(map))
            {
                CollectionAssert.AreEqual(new List<(short X, short Y)> { (1, 1) },
                    pathfinder.FindPath((0, 0), (1, 1)).ToList(), pathfinder.GetType().Name);
            }
        }

        [TestMethod]
        public void AnUnreachableGoalIsRejectedWithoutSearchingTheMapAgain()
        {
            var cells = Enumerable.Range(0, 100).Select(_ => new byte[100]).ToArray();
            for (var i = 47; i <= 53; i++)
            {
                cells[47][i] = cells[53][i] = cells[i][47] = cells[i][53] = 1;
            }

            var map = new CountingMap(cells);
            var pathfinder = new JumpPointSearchPathfinder(map, new OctileDistanceHeuristic());
            Assert.IsEmpty(pathfinder.FindPath((2, 2), (50, 50)).ToList());

            map.Reads = 0;
            Assert.IsEmpty(pathfinder.FindPath((3, 2), (50, 50)).ToList());

            Assert.IsLessThan(10, map.Reads, "a goal walled off from the start must not trigger another full search");
        }

        [TestMethod]
        public void IsReachableIsFalseOutsideTheMapOrOnWalls()
        {
            Assert.IsFalse(Open.IsReachable((-1, 0), (0, 0)));
            Assert.IsFalse(Open.IsReachable((0, 0), (4, 0)));
            Assert.IsFalse(Squeeze.IsReachable((0, 0), (1, 0)));
            Assert.IsFalse(Squeeze.IsReachable((0, 0), (1, 1)));
            Assert.IsTrue(Open.IsReachable((0, 0), (3, 2)));
            Assert.IsTrue(Open.IsReachable((1, 1), (1, 1)));
        }

        private sealed class CountingMap : IMapGrid
        {
            private readonly TestMap _map;

            public CountingMap(byte[][] cells)
            {
                _map = new TestMap(cells);
            }

            public int Reads { get; set; }

            public short Width => _map.Width;

            public short Height => _map.Height;

            public byte this[short x, short y] => _map[x, y];

            public bool IsWalkable(short currentX, short currentY)
            {
                Reads++;
                return _map.IsWalkable(currentX, currentY);
            }
        }
    }
}
