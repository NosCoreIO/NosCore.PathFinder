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
            var scale = 50;
            var bitmap = new Bitmap(_map.XLength * scale, _map.YLength * scale);
            using var graphics = Graphics.FromImage(bitmap);
            var listPixel = new List<Color>();
            for (short y = 0; y < _map.YLength; y++)
            {
                for (short x = 0; x < _map.XLength; x++)
                {
                    var color = (brushFire[x, y] ?? 0d) == 0 ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb((int)(((brushFire[x, y] ?? 0) * 12 > 255 ? 255 : (brushFire[x, y] ?? 0) * 12)), 0, 0, 255);
                    if (x == characterPosition.X && y == characterPosition.Y)
                    {
                        color = Color.DarkRed;
                    }
                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    var rectangle = new Rectangle(x * scale, y * scale, scale, scale);
                    graphics.DrawString(brushFire[x, y]?.ToString("N0") ?? "∞", new Font("Arial", 16), Brushes.Black, rectangle, sf);
                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                    listPixel.Add(color);
                }
            }

            var path = Path.GetFullPath("../../../../../documentation/brushfire.png");
            bitmap.Save(path, ImageFormat.Png);

            var builder = new StringBuilder();
            builder.AppendLine("# NosCore.Pathfinder's Documentation");
            builder.AppendLine("## Brushfire");
            builder.AppendLine("- Filename: brushfire.png");
            var pixels = string.Join("", listPixel.SelectMany(s => s.Name));

            var checksum =
                string.Join("", SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(pixels)).Select(s => s.ToString("x2")));
            builder.AppendLine($"- Checksum: {checksum}");
            builder.AppendLine("![brushfire](./brushfire.png)");
            Approvals.Verify(WriterFactory.CreateTextWriter(builder.ToString(), "md"));
        }
    }
}
