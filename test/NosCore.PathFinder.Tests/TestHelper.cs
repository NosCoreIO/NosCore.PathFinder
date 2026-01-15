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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NosCore.PathFinder.Tests
{
    public static class TestHelper
    {
        public static void VerifyFile(string linearPathfinderPng, Image<Rgba32> image, List<Rgba32> listPixel, string desc)
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

        private static Font? _font;
        private static bool _fontLoaded;
        public static Font? GetFont()
        {
            if (!_fontLoaded)
            {
                _fontLoaded = true;
                var fontNames = new[] { "Arial", "DejaVu Sans", "Liberation Sans", "Noto Sans", "FreeSans" };
                foreach (var fontName in fontNames)
                {
                    if (SystemFonts.TryGet(fontName, out var family))
                    {
                        _font = family.CreateFont(16);
                        break;
                    }
                }

                _font ??= SystemFonts.Collection.Families.FirstOrDefault()?.CreateFont(16);
            }
            return _font;
        }

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

        public static void DrawMap(TestMap map, int scale, List<Rgba32> listPixel, Image<Rgba32> image, (short X, short Y) monster, (short X, short Y) character)
        {
            var font = GetFont();
            image.Mutate(ctx =>
            {
                for (short y = 0; y < map.Height; y++)
                {
                    for (short x = 0; x < map.Width; x++)
                    {
                        var rect = new RectangleF(x * scale, y * scale, scale, scale);
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
                        ctx.Fill(color, rect);
                        if (text != null && font != null)
                        {
                            var textOptions = new RichTextOptions(font)
                            {
                                Origin = new PointF(x * scale + scale / 2f, y * scale + scale / 2f),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            ctx.DrawText(textOptions, text, Color.Black);
                        }

                        listPixel.Add(color.ToPixel<Rgba32>());
                    }
                }
            });
        }

        public static void DrawArrow(Image<Rgba32> image, int cellX, int cellY, float dirX, float dirY, int scale, Rgba32 color)
        {
            var centerX = cellX * scale + scale / 2f;
            var centerY = cellY * scale + scale / 2f;

            var arrowLength = scale * 0.35f;
            var headLength = scale * 0.15f;

            var endX = centerX + dirX * arrowLength;
            var endY = centerY + dirY * arrowLength;

            image.Mutate(ctx =>
            {
                ctx.DrawLine(color, 2f, new PointF(centerX, centerY), new PointF(endX, endY));

                var angle = (float)Math.Atan2(dirY, dirX);
                var head1X = endX - headLength * (float)Math.Cos(angle - 0.5f);
                var head1Y = endY - headLength * (float)Math.Sin(angle - 0.5f);
                var head2X = endX - headLength * (float)Math.Cos(angle + 0.5f);
                var head2Y = endY - headLength * (float)Math.Sin(angle + 0.5f);

                var headPoints = new PointF[] { new(endX, endY), new(head1X, head1Y), new(head2X, head2Y) };
                ctx.FillPolygon(color, headPoints);
            });
        }
    }
}
