//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class BrushFireTests
    {
        private readonly TestMap _map = new TestMap(new[]
        {
            new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        });


        [TestMethod]
        public void Test_BrushFire()
        {
            (short X, short Y) characterPosition = (6, 10);
            var brushFire = _map.LoadBrushFire(characterPosition, new OctileDistanceHeuristic());
            var bitmap = new Bitmap(_map.XLength * TestHelper.Scale, _map.YLength * TestHelper.Scale);
            var listPixel = new List<Color>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, bitmap, (0, 0), characterPosition);
            using var graphics = Graphics.FromImage(bitmap);


            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    var rectangle = new Rectangle(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale);
                    if ((x, y) != characterPosition)
                    {
                        if (brushFire[x, y] != null)
                        {
                            graphics.FillRectangle(new Pen(Color.White).Brush, rectangle);
                            var color = Color.FromArgb((int)((brushFire[x, y] * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12)), 0, 0, 255);
                            graphics.DrawString(brushFire[x, y]?.ToString("N0"), new Font("Arial", 16), Brushes.Black, rectangle, TestHelper.StringFormat);
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

            TestHelper.VerifyFile("brushfire.png", bitmap, listPixel, "Brushfire");
        }
    }
}
