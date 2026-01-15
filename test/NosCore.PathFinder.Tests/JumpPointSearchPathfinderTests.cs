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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
            using var image = new Image<Rgba32>(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            (short X, short Y) target = (15, 16);
            var listPixel = new List<Rgba32>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, target, _characterPosition);
            var font = TestHelper.GetFont();

            var jumps = _jumpPointSearchPathfinder.GetJumpList(target, _characterPosition).ToList();
            var path = _jumpPointSearchPathfinder.FindPath(target, _characterPosition).ToList();

            image.Mutate(ctx =>
            {
                foreach (var (x, y) in path)
                {
                    if ((x, y) != target && (x, y) != _characterPosition)
                    {
                        var rect = new RectangleF(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                        var color = Color.LightPink;
                        if (jumps.Contains((x, y)))
                        {
                            color = Color.DeepPink;
                        }

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

            TestHelper.VerifyFile("jump-point-search-pathfinder.png", image, listPixel, "Jump Point Search Pathfinder (break at walls)");
        }
    }
}
