//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Interfaces;
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class IPathfinderTest
    {
        private readonly TestMap _map = new TestMap(new[]
        {
            new byte[] {1, 1, 1, 1, 1, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 0, 0, 0, 0, 1, },
            new byte[] {1, 1, 1, 1, 1, 1, },
        });

        [TestMethod]
        public void AllPathfinderShouldReturnEnd()
        {
            var pathfinders = new List<IPathfinder>
            {
                new GoalBasedPathfinder(_map, new OctileDistanceHeuristic()),
                new JumpPointSearchPathfinder(_map, new OctileDistanceHeuristic()),
            };

            var pathfindersTypes = typeof(IPathfinder).Assembly
                .GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(IPathfinder))).ToList();

            Assert.IsTrue(!pathfindersTypes.Except(pathfinders.Select(s => s.GetType())).Any());

            foreach (var pathfinder in pathfinders)
            {
                var path = pathfinder.FindPath((1, 1), (4, 6)).ToList();
                var last = path.Last();
                Assert.AreEqual((4, 6), last);
            }
        }
    }
}
