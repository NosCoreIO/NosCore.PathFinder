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
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Tests
{
    [TestClass]
    public class GoalBasedPathfinderTests
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
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            new byte[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        });

        private readonly IPathfinder _goalPathfinder;
        private readonly Cell _characterPosition;
        private readonly Cell?[,] _brushFire;

        public GoalBasedPathfinderTests()
        {
            _characterPosition = new Cell(6, 10);
            _brushFire = _map.LoadBrushFire(_characterPosition, new OctileDistanceHeuristic());
            _goalPathfinder = new GoalBasedPathfinder(_brushFire, _map);
        }

        [TestMethod]
        public void Test_GoalBasedPathfinder()
        {
            var scale = 50;
            var bitmap = new Bitmap(_map.XLength * scale, _map.YLength * scale);
            using var graphics = Graphics.FromImage(bitmap);
            var listPixel = new List<Color>();
            var target = new Cell(15, 16);
            var path = _goalPathfinder.FindPath(_characterPosition, target);
            for (var y = 0; y < _map.YLength; y++)
            {
                for (var x = 0; x < _map.XLength; x++)
                {
                    var color = (_brushFire[x, y]?.Value ?? 0d) == 0 ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb((int)(((_brushFire[x, y]?.Value ?? 0) * 12 > 255 ? 255 : (_brushFire[x, y]?.Value ?? 0) * 12)), 0, 0, 255);
                    if (x == _characterPosition.X && y == _characterPosition.Y)
                    {
                        color = Color.Green;
                    }

                    if (path.Any(s => s.X == x && s.Y == y))
                    {
                        color = Color.FromArgb(100, Color.Purple);
                    }

                    if (x == target.X && y == target.Y)
                    {
                        color = Color.FromArgb(200, Color.DarkRed);
                    }

                    var sf = new StringFormat
                    {
                        LineAlignment = StringAlignment.Center,
                        Alignment = StringAlignment.Center
                    };
                    var rectangle = new Rectangle(x * scale, y * scale, scale, scale);
                    graphics.DrawString(_brushFire[x, y]?.Value.ToString("N0") ?? "∞", new Font("Arial", 16), Brushes.Black, rectangle, sf);
                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                    listPixel.Add(color);
                }
            }

            var filepath = Path.GetFullPath("../../../../../documentation/goal-based-pathfinder.png");
            bitmap.Save(filepath, ImageFormat.Png);

            var builder = new StringBuilder();
            builder.AppendLine("# NosCore.Pathfinder's Documentation");
            builder.AppendLine("## Goal Based Pathfinder");
            builder.AppendLine("- Filename: goal-based-pathfinder.png");
            var pixels = string.Join("", listPixel.SelectMany(s => s.Name));

            var checksum =
                string.Join("", SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(pixels)).Select(s => s.ToString("x2")));
            builder.AppendLine($"- Checksum: {checksum}");
            builder.AppendLine("![brushfire](./goal-based-pathfinder.png)");
            Approvals.Verify(WriterFactory.CreateTextWriter(builder.ToString(), "md"));
        }
    }
}
