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
    public class GoalBasedPathfinderTests
    {
        private readonly TestMap _map = TestHelper.SimpleMap;

        private readonly IPathfinder _goalPathfinder;
        private readonly (short X, short Y) _characterPosition;
        private readonly BrushFire _brushFire;

        public GoalBasedPathfinderTests()
        {
            _characterPosition = (6, 10);
            _brushFire = _map.LoadBrushFire(_characterPosition, new OctileDistanceHeuristic());
            _goalPathfinder = new GoalBasedPathfinder(_map, new OctileDistanceHeuristic());
        }

        [TestMethod]
        public void Test_GoalBasedPathfinder()
        {
            using var image = new MapCanvas(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Rgba>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, target, _characterPosition);

            for (short y = 0; y < _map.Height; y++)
            {
                for (short x = 0; x < _map.Width; x++)
                {
                    if ((x, y) == target || (x, y) == _characterPosition)
                    {
                        continue;
                    }

                    var centerX = x * TestHelper.Scale + TestHelper.Scale / 2f;
                    var centerY = y * TestHelper.Scale + TestHelper.Scale / 2f;
                    if (_brushFire[x, y] != null)
                    {
                        image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, Colors.White);
                        var alpha = (byte)((_brushFire[x, y] * 12 > 255 ? 255 : (_brushFire[x, y] ?? 0) * 12));
                        var color = new Rgba(0, 0, 255, alpha);
                        image.DrawTextCentered(centerX, centerY, _brushFire[x, y]?.ToString("N0") ?? "", Colors.Black);
                        image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, color);
                        listPixel.Add(color);
                    }
                    else
                    {
                        image.DrawTextCentered(centerX, centerY, "∞", Colors.White);
                    }
                }
            }

            var path = _goalPathfinder.FindPath(target, _characterPosition).ToList();
            foreach (var (x, y) in path)
            {
                if ((x, y) != target && (x, y) != _characterPosition)
                {
                    var color = Colors.LightPink;
                    image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, color);
                    image.DrawTextCentered(x * TestHelper.Scale + TestHelper.Scale / 2f,
                        y * TestHelper.Scale + TestHelper.Scale / 2f,
                        Array.IndexOf(path.ToArray(), (x, y)).ToString(), Colors.Black);
                    listPixel.Add(color);
                }
            }

            TestHelper.VerifyFile("goal-based-pathfinder.png", image, listPixel, "Goal Based Pathfinder");
        }


        [TestMethod]
        public void Test_GoalBasedPathfinder_OutOfDistance_ShouldNotReturnPath()
        {
            (short X, short Y) characterPosition = (6, 10);
            var brushFire = _map.LoadBrushFire(characterPosition, new OctileDistanceHeuristic(), 2);
            var goalPathfinder = new GoalBasedPathfinder(_map, new OctileDistanceHeuristic(), brushFire);
            var path = goalPathfinder.FindPath((2, 2), characterPosition);
            Assert.AreEqual(0, path.Count());
        }
    }
}
