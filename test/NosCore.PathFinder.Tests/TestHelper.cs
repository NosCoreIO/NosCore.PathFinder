//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public static void VerifyFile(string linearPathfinderPng, Bitmap bitmap, List<Color> listPixel, string desc)
        {
            var filepath = Path.GetFullPath($"../../../../../documentation/{linearPathfinderPng}");
            bitmap.Save(filepath, ImageFormat.Png);

            var builder = new StringBuilder();
            builder.AppendLine("# NosCore.Pathfinder's Documentation");
            builder.AppendLine($"## {desc}");
            builder.AppendLine($"- Filename: {linearPathfinderPng}");
            var pixels = string.Join("", listPixel.SelectMany(s => s.Name));

            var checksum =
                string.Join("", SHA256.Create()
                    .ComputeHash(Encoding.UTF8.GetBytes(pixels)).Select(s => s.ToString("x2")));
            builder.AppendLine($"- Checksum: {checksum}");
            builder.AppendLine($"![brushfire](./{linearPathfinderPng})");
            Approvals.Verify(WriterFactory.CreateTextWriter(builder.ToString(), "md"));
        }


        public static int Scale = 50;
        public static StringFormat StringFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };

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

        public static void DrawMap(TestMap map, int scale, List<Color> listPixel, Bitmap bitmap, (short X, short Y) monster, (short X, short Y) character)
        {
            using var graphics = Graphics.FromImage(bitmap);
            for (short y = 0; y < map.YLength; y++)
            {
                for (short x = 0; x < map.XLength; x++)
                {
                    var rectangle = new Rectangle(x * scale, y * scale, scale, scale);
                    var color = Color.Blue;
                    string? text = null;
                    if (!map.IsWalkable(x, y))
                    {
                        color = Color.Black;
                    }

                    if (character == (x, y))
                    {
                        text = "P";
                        color = Color.Green;
                    }

                    if (monster != default && monster == (x, y))
                    {
                        text = "M";
                        color = Color.DarkRed;
                    }
                    graphics.FillRectangle(new Pen(color).Brush, rectangle);
                    graphics.DrawString(text, new Font("Arial", 16), Brushes.Black, rectangle, StringFormat);

                    listPixel.Add(color);
                }
            }
        }
    }
}
