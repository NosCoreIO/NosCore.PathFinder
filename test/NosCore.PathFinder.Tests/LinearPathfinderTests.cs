using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Heuristic;
using NosCore.PathFinder.Interfaces;
using NosCore.PathFinder.Pathfinder;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class LinearPathfinderTests
    {
        private readonly TestMap _map = TestHelper.SimpleMap;

        private readonly IPathfinder _linearPathfinder;
        private readonly (short X, short Y) _characterPosition;

        public LinearPathfinderTests()
        {
            _characterPosition = (6, 10);
#pragma warning disable CS0618 // Type or member is obsolete
            _linearPathfinder = new LinearPathfinder(_map, new OctileDistanceHeuristic());
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [TestMethod]
        public void Test_LinearPathfinder()
        {
            var bitmap = new Bitmap(_map.XLength * TestHelper.Scale, _map.YLength * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, target, _characterPosition);
            using var graphics = Graphics.FromImage(bitmap);

            var path = _linearPathfinder.FindPath(target, _characterPosition).ToList();
            foreach (var (x, y) in path)
            {
                var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                var color = Color.LightPink;
                graphics.FillRectangle(new Pen(color).Brush, rectangle);
                graphics.DrawString(Array.IndexOf(path.ToArray(), (x, y)).ToString(), new Font("Arial", 16), Brushes.Black, rectangle, TestHelper.StringFormat);
                listPixel.Add(color);
            }
            TestHelper.VerifyFile("linear-pathfinder.png", bitmap, listPixel, "Linear Pathfinder (break at walls)");
        }
    }
}
