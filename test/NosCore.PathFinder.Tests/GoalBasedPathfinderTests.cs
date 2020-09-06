using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApprovalTests;
using ApprovalTests.Writers;
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
            var bitmap = new Bitmap(_map.XLength * TestHelper.Scale, _map.YLength * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, target, _characterPosition);
            using var graphics = Graphics.FromImage(bitmap);


            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                    if ((x, y) != target && (x, y) != _characterPosition)
                    {
                        if (_brushFire[x, y] != null)
                        {
                            graphics.FillRectangle(new Pen(Color.White).Brush, rectangle);
                            var color = Color.FromArgb((int)((_brushFire[x, y] * 12 > 255 ? 255 : (_brushFire[x, y] ?? 0) * 12)), 0, 0, 255);
                            graphics.DrawString(_brushFire[x, y]?.ToString("N0"), new Font("Arial", 16), Brushes.Black, rectangle, TestHelper.StringFormat);
                            graphics.FillRectangle(new Pen(color).Brush, rectangle);
                            listPixel.Add(color);
                        }
                        else
                        {
                            graphics.DrawString("∞", new Font("Arial", 16), Brushes.White, rectangle, TestHelper.StringFormat);
                        }
                    }
                }
            }


            var path = _goalPathfinder.FindPath(target, _characterPosition).ToList();
            foreach (var (x, y) in path)
            {
                var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                var color = Color.LightPink;
                graphics.FillRectangle(new Pen(color).Brush, rectangle);
                graphics.DrawString(Array.IndexOf(path.ToArray(), (x, y)).ToString(), new Font("Arial", 16), Brushes.Black, rectangle, TestHelper.StringFormat);
                listPixel.Add(color);
            }

            TestHelper.VerifyFile("goal-based-pathfinder.png", bitmap, listPixel, "Goal Based Pathfinder");
        }
    }
}
