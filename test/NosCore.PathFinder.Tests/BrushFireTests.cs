//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class BrushFireTests
    {
        private readonly TestMap _map = TestHelper.SimpleMap;


        [TestMethod]
        public void Test_BrushFire()
        {
            (short X, short Y) characterPosition = (6, 10);
            var brushFire = _map.LoadBrushFire(characterPosition, new OctileDistanceHeuristic());
            using var image = new Image<Rgba32>(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            var listPixel = new List<Rgba32>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, (0, 0), characterPosition);
            var font = TestHelper.GetFont();

            image.Mutate(ctx =>
            {
                for (short y = 0; y < _map.Height; y++)
                {
                    for (short x = 0; x < _map.Width; x++)
                    {
                        var rect = new RectangleF(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                        if ((x, y) != characterPosition)
                        {
                            if (brushFire[x, y] != null)
                            {
                                ctx.Fill(Color.White, rect);
                                var alpha = (byte)((brushFire[x, y] * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12));
                                var color = new Rgba32(0, 0, 255, alpha);
                                var textOptions = new RichTextOptions(font)
                                {
                                    Origin = new PointF(x * TestHelper.Scale + TestHelper.Scale / 2f, y * TestHelper.Scale + TestHelper.Scale / 2f),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };
                                ctx.DrawText(textOptions, brushFire[x, y]?.ToString("N0") ?? "", Color.Black);
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

            TestHelper.VerifyFile("brushfire.png", image, listPixel, "Brushfire");
        }
    }
}
