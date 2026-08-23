//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class JumpPointSearchPathfinderTests
    {
        private readonly TestMap _map = TestHelper.SimpleMap;

        private readonly JumpPointSearchPathfinder _jumpPointSearchPathfinder;
        private readonly (short X, short Y) _characterPosition;

        public JumpPointSearchPathfinderTests()
        {
            _characterPosition = (6, 10);
            _jumpPointSearchPathfinder = new JumpPointSearchPathfinder(_map, new OctileDistanceHeuristic());
        }

        [TestMethod]
        public void Test_JumpPointSearchPathfinder()
        {
            using var image = new MapCanvas(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Rgba>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, target, _characterPosition);

            var jumps = _jumpPointSearchPathfinder.GetJumpList(target, _characterPosition).ToList();
            var path = _jumpPointSearchPathfinder.FindPath(target, _characterPosition).ToList();

            foreach (var (x, y) in path)
            {
                if ((x, y) != target && (x, y) != _characterPosition)
                {
                    var color = jumps.Contains((x, y)) ? Colors.DeepPink : Colors.LightPink;
                    image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, color);
                    image.DrawTextCentered(x * TestHelper.Scale + TestHelper.Scale / 2f,
                        y * TestHelper.Scale + TestHelper.Scale / 2f,
                        Array.IndexOf(path.ToArray(), (x, y)).ToString(), Colors.Black);
                    listPixel.Add(color);
                }
            }

            TestHelper.VerifyFile("jump-point-search-pathfinder.png", image, listPixel, "Jump Point Search Pathfinder (break at walls)");
        }
    }
}
