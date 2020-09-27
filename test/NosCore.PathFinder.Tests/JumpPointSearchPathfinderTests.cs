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
            var bitmap = new Bitmap(_map.Width * TestHelper.Scale, _map.Length * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, target, _characterPosition);
            using var graphics = Graphics.FromImage(bitmap);

            var jumps = _jumpPointSearchPathfinder.GetJumpList(target, _characterPosition).ToList();
            var path = _jumpPointSearchPathfinder.FindPath(target, _characterPosition).ToList();
            foreach (var (x, y) in path)
            {
                if ((x, y) != target && (x, y) != _characterPosition)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale,
                        TestHelper.Scale);
                    var color = Color.LightPink;
                    if (jumps.Contains((x, y)))
                    {
                        color = Color.DeepPink;
                    }

                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                    graphics.DrawString(Array.IndexOf(path.ToArray(), (x, y)).ToString(), new Font("Arial", 16),
                        Brushes.Black, rectangle, TestHelper.StringFormat);
                    listPixel.Add(color);
                }
            }
            TestHelper.VerifyFile("jump-point-search-pathfinder.png", bitmap, listPixel, "Jump Point Search Pathfinder (break at walls)");
        }
    }
}
