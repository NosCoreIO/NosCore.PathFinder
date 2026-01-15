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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
            using var image = new Image<Rgba32>(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Rgba32>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, target, _characterPosition);
            var font = TestHelper.GetFont();

            image.Mutate(ctx =>
            {
                for (short y = 0; y < _map.Height; y++)
                {
                    for (short x = 0; x < _map.Width; x++)
                    {
                        var rect = new RectangleF(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                        if ((x, y) != target && (x, y) != _characterPosition)
                        {
                            if (_brushFire[x, y] != null)
                            {
                                ctx.Fill(Color.White, rect);
                                var alpha = (byte)((_brushFire[x, y] * 12 > 255 ? 255 : (_brushFire[x, y] ?? 0) * 12));
                                var color = new Rgba32(0, 0, 255, alpha);
                                var textOptions = new RichTextOptions(font)
                                {
                                    Origin = new PointF(x * TestHelper.Scale + TestHelper.Scale / 2f, y * TestHelper.Scale + TestHelper.Scale / 2f),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                ctx.DrawText(textOptions, _brushFire[x, y]?.ToString("N0") ?? "", Color.Black);
                                ctx.Fill(color, rect);
                                listPixel.Add(color);
                            }
                            else
                            {
                                var textOptions = new RichTextOptions(font)
                                {
                                    Origin = new PointF(x * TestHelper.Scale + TestHelper.Scale / 2f, y * TestHelper.Scale + TestHelper.Scale / 2f),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                ctx.DrawText(textOptions, "∞", Color.White);
                            }
                        }
                    }
                }
            });


            var path = _goalPathfinder.FindPath(target, _characterPosition).ToList();
            image.Mutate(ctx =>
            {
                foreach (var (x, y) in path)
                {
                    if ((x, y) != target && (x, y) != _characterPosition)
                    {
                        var rect = new RectangleF(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                        var color = Color.LightPink;
                        ctx.Fill(color, rect);
                        var textOptions = new RichTextOptions(font)
                        {
                            Origin = new PointF(x * TestHelper.Scale + TestHelper.Scale / 2f, y * TestHelper.Scale + TestHelper.Scale / 2f),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        ctx.DrawText(textOptions, Array.IndexOf(path.ToArray(), (x, y)).ToString(), Color.Black);
                        listPixel.Add(color.ToPixel<Rgba32>());
                    }
                }
            });

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
