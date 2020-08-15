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
using Moq;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class BrushFireTest
    {
        private TestMap _map;

        [TestInitialize]
        public void Setup()
        {
            _map = new TestMap(new[]
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
        }


        [TestMethod]
        public void Test_BrushFire()
        {
            var characterPosition = new MapCell(6, 10);
            var brushFire = _map.LoadBrushFire(characterPosition, new OctileHeuristic());
            var scale = 50;
            var bitmap = new Bitmap(_map.XLength * scale, _map.YLength * scale);
            using var graphics = Graphics.FromImage(bitmap);
            for (var y = 0; y < _map.YLength; y++)
            {
                for (var x = 0; x < _map.XLength; x++)
                {
                    var color = (brushFire[x, y]?.F ?? 0) == 0 ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb((int)(255 / (brushFire[x, y].F / 5 + 1)), 0, 0, 255);
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
                    graphics.DrawString(brushFire[x, y]?.F.ToString("N0") ?? "∞", new Font("Arial", 16), Brushes.Black, rectangle, sf);
                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                }
            }

            var path = Path.GetFullPath("../../../../../documentation/brushfire.png");
            var listPixel = new List<Color>();
            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    listPixel.Add(bitmap.GetPixel(x, y));
                }
            }

            var builder = new StringBuilder();
            builder.AppendLine("# NosCore.Pathfinder's Documentation");
            builder.AppendLine("## Brushfire");
            builder.AppendLine("- Filename: brushfire.png");
            var checksum = string.Join("",
                SHA256.Create()
                    .ComputeHash(listPixel.SelectMany(s => Encoding.UTF8.GetBytes($"{s.A}{s.R}{s.G}{s.B}")).ToArray())
                    .Select(s => s.ToString("x2")));
            Console.WriteLine(checksum);
            builder.AppendLine($"- Checksum: {checksum}");
            builder.AppendLine("![brushfire](./brushfire.png)");
            Approvals.Verify(WriterFactory.CreateTextWriter(builder.ToString(), "md"));
        }
    }
}
