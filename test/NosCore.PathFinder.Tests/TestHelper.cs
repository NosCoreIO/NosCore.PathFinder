//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApprovalTests;
using ApprovalTests.Writers;

namespace NosCore.PathFinder.Tests
{
    public static class TestHelper
    {
        public static void VerifyFile(string linearPathfinderPng, MapCanvas image, List<Rgba> listPixel, string desc)
        {
            var filepath = Path.GetFullPath($"../../../../../documentation/{linearPathfinderPng}");
            image.SaveAsPng(filepath);

            var builder = new StringBuilder();
            builder.AppendLine("# NosCore.Pathfinder's Documentation");
            builder.AppendLine($"## {desc}");
            builder.AppendLine($"- Filename: {linearPathfinderPng}");
            var pixels = string.Join("", listPixel.SelectMany(s => $"{s.R:X2}{s.G:X2}{s.B:X2}{s.A:X2}"));

            var checksum =
                string.Join("", SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(pixels)).Select(s => s.ToString("x2")));
            builder.AppendLine($"- Checksum: {checksum}");
            builder.AppendLine($"![brushfire](./{linearPathfinderPng})");
            Approvals.Verify(WriterFactory.CreateTextWriter(builder.ToString(), "md"));
        }

        public static int Scale = 50;

        public static TestMap SimpleMap = new TestMap(new[]
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

        public static void DrawMap(TestMap map, int scale, List<Rgba> listPixel, MapCanvas image, (short X, short Y) monster, (short X, short Y) character)
        {
            for (short y = 0; y < map.Height; y++)
            {
                for (short x = 0; x < map.Width; x++)
                {
                    var color = Colors.Blue;
                    string? text = null;
                    if (!map.IsWalkable(x, y))
                    {
                        color = Colors.Black;
                    }

                    if (character == (x, y))
                    {
                        text = "P";
                        color = Colors.Green;
                    }

                    if (monster != default && monster == (x, y))
                    {
                        text = "M";
                        color = Colors.DarkRed;
                    }

                    image.FillRect(x * scale, y * scale, scale, scale, color);
                    if (text != null)
                    {
                        image.DrawTextCentered(x * scale + scale / 2f, y * scale + scale / 2f, text, Colors.Black);
                    }

                    listPixel.Add(color);
                }
            }
        }

        public static void DrawArrow(MapCanvas image, int cellX, int cellY, float dirX, float dirY, int scale, Rgba color)
        {
            var centerX = cellX * scale + scale / 2f;
            var centerY = cellY * scale + scale / 2f;

            var arrowLength = scale * 0.35f;
            var headLength = scale * 0.15f;

            var endX = centerX + dirX * arrowLength;
            var endY = centerY + dirY * arrowLength;

            image.DrawLine(centerX, centerY, endX, endY, 2f, color);

            var angle = (float)Math.Atan2(dirY, dirX);
            var head1X = endX - headLength * (float)Math.Cos(angle - 0.5f);
            var head1Y = endY - headLength * (float)Math.Sin(angle - 0.5f);
            var head2X = endX - headLength * (float)Math.Cos(angle + 0.5f);
            var head2Y = endY - headLength * (float)Math.Sin(angle + 0.5f);

            image.FillPolygon(new[] { (endX, endY), (head1X, head1Y), (head2X, head2Y) }, color);
        }
    }
}
