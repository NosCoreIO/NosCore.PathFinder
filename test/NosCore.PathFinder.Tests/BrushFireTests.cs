//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Heuristic;

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
            var image = new MapCanvas(_map.Width * TestHelper.Scale, _map.Height * TestHelper.Scale);
            var listPixel = new List<Rgba>();
            TestHelper.DrawMap(_map, TestHelper.Scale, listPixel, image, (0, 0), characterPosition);

            for (short y = 0; y < _map.Height; y++)
            {
                for (short x = 0; x < _map.Width; x++)
                {
                    if ((x, y) == characterPosition)
                    {
                        continue;
                    }

                    var centerX = x * TestHelper.Scale + TestHelper.Scale / 2f;
                    var centerY = y * TestHelper.Scale + TestHelper.Scale / 2f;
                    if (brushFire[x, y] != null)
                    {
                        image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, Colors.White);
                        var alpha = (byte)((brushFire[x, y] * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12));
                        var color = new Rgba(0, 0, 255, alpha);
                        image.DrawTextCentered(centerX, centerY, brushFire[x, y]?.ToString("N0") ?? "", Colors.Black);
                        image.FillRect(x * TestHelper.Scale, y * TestHelper.Scale, TestHelper.Scale, TestHelper.Scale, color);
                        listPixel.Add(color);
                    }
                    else
                    {
                        image.DrawTextCentered(centerX, centerY, "∞", Colors.White);
                    }
                }
            }

            TestHelper.VerifyFile("brushfire.png", image, listPixel, "Brushfire");
        }
    }
}
