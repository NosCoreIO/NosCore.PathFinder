//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using NetVips;

namespace NosCore.PathFinder.Tests
{
    public readonly record struct Rgba(byte R, byte G, byte B, byte A)
    {
        public Rgba(byte r, byte g, byte b) : this(r, g, b, 255)
        {
        }
    }

    public static class Colors
    {
        public static readonly Rgba Black = new(0, 0, 0);
        public static readonly Rgba White = new(255, 255, 255);
        public static readonly Rgba Blue = new(0, 0, 255);
        public static readonly Rgba Green = new(0, 128, 0);
        public static readonly Rgba DarkRed = new(139, 0, 0);
        public static readonly Rgba LightPink = new(255, 182, 193);
        public static readonly Rgba DeepPink = new(255, 20, 147);
    }

    /// <summary>
    /// A plain RGBA byte surface. Compositing is done here because the shapes are
    /// axis-aligned and trivial; libvips is used for the two parts that genuinely need
    /// a library, glyph rasterisation and PNG encoding.
    /// </summary>
    public sealed class MapCanvas
    {
        private const string FontSpec = "sans 16";

        private readonly byte[] _pixels;
        private static bool _fontUnavailable;

        public MapCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new byte[width * height * 4];
        }

        public int Width { get; }
        public int Height { get; }

        public void FillRect(float x, float y, float width, float height, Rgba color)
        {
            var left = Math.Max(0, (int)MathF.Round(x));
            var top = Math.Max(0, (int)MathF.Round(y));
            var right = Math.Min(Width, (int)MathF.Round(x + width));
            var bottom = Math.Min(Height, (int)MathF.Round(y + height));

            for (var py = top; py < bottom; py++)
            {
                for (var px = left; px < right; px++)
                {
                    Blend(px, py, color, color.A);
                }
            }
        }

        public void DrawLine(float x1, float y1, float x2, float y2, float thickness, Rgba color)
        {
            var radius = thickness / 2f;
            var left = Math.Max(0, (int)MathF.Floor(Math.Min(x1, x2) - radius));
            var top = Math.Max(0, (int)MathF.Floor(Math.Min(y1, y2) - radius));
            var right = Math.Min(Width - 1, (int)MathF.Ceiling(Math.Max(x1, x2) + radius));
            var bottom = Math.Min(Height - 1, (int)MathF.Ceiling(Math.Max(y1, y2) + radius));

            for (var py = top; py <= bottom; py++)
            {
                for (var px = left; px <= right; px++)
                {
                    if (DistanceToSegment(px + 0.5f, py + 0.5f, x1, y1, x2, y2) <= radius)
                    {
                        Blend(px, py, color, color.A);
                    }
                }
            }
        }

        public void FillPolygon(ReadOnlySpan<(float X, float Y)> points, Rgba color)
        {
            var minX = Width - 1;
            var minY = Height - 1;
            var maxX = 0;
            var maxY = 0;
            foreach (var (x, y) in points)
            {
                minX = Math.Min(minX, Math.Max(0, (int)MathF.Floor(x)));
                minY = Math.Min(minY, Math.Max(0, (int)MathF.Floor(y)));
                maxX = Math.Max(maxX, Math.Min(Width - 1, (int)MathF.Ceiling(x)));
                maxY = Math.Max(maxY, Math.Min(Height - 1, (int)MathF.Ceiling(y)));
            }

            for (var py = minY; py <= maxY; py++)
            {
                for (var px = minX; px <= maxX; px++)
                {
                    if (Contains(points, px + 0.5f, py + 0.5f))
                    {
                        Blend(px, py, color, color.A);
                    }
                }
            }
        }

        public void DrawTextCentered(float centerX, float centerY, string text, Rgba color)
        {
            if (_fontUnavailable || string.IsNullOrEmpty(text))
            {
                return;
            }

            Image glyphs;
            try
            {
                glyphs = Image.Text(text, font: FontSpec, dpi: 72);
            }
            catch (VipsException)
            {
                _fontUnavailable = true;
                return;
            }

            using (glyphs)
            {
                var mask = glyphs.WriteToMemory<byte>();
                var originX = (int)MathF.Round(centerX - glyphs.Width / 2f);
                var originY = (int)MathF.Round(centerY - glyphs.Height / 2f);

                for (var gy = 0; gy < glyphs.Height; gy++)
                {
                    for (var gx = 0; gx < glyphs.Width; gx++)
                    {
                        var coverage = mask[gy * glyphs.Width + gx];
                        if (coverage == 0)
                        {
                            continue;
                        }

                        var px = originX + gx;
                        var py = originY + gy;
                        if (px < 0 || py < 0 || px >= Width || py >= Height)
                        {
                            continue;
                        }

                        Blend(px, py, color, (byte)(color.A * coverage / 255));
                    }
                }
            }
        }

        public void SaveAsPng(string path)
        {
            using var image = Image.NewFromMemory(_pixels, Width, Height, 4, Enums.BandFormat.Uchar)
                .Copy(interpretation: Enums.Interpretation.Srgb);
            image.WriteToFile(path);
        }

        private void Blend(int x, int y, Rgba source, byte sourceAlpha)
        {
            var i = (y * Width + x) * 4;
            if (sourceAlpha == 255)
            {
                _pixels[i] = source.R;
                _pixels[i + 1] = source.G;
                _pixels[i + 2] = source.B;
                _pixels[i + 3] = 255;
                return;
            }

            var sa = sourceAlpha / 255f;
            var da = _pixels[i + 3] / 255f;
            var outA = sa + da * (1 - sa);
            if (outA <= 0)
            {
                return;
            }

            _pixels[i] = Composite(source.R, _pixels[i], sa, da, outA);
            _pixels[i + 1] = Composite(source.G, _pixels[i + 1], sa, da, outA);
            _pixels[i + 2] = Composite(source.B, _pixels[i + 2], sa, da, outA);
            _pixels[i + 3] = (byte)MathF.Round(outA * 255);
        }

        private static byte Composite(byte source, byte destination, float sa, float da, float outA) =>
            (byte)MathF.Round((source * sa + destination * da * (1 - sa)) / outA);

        private static float DistanceToSegment(float px, float py, float x1, float y1, float x2, float y2)
        {
            var dx = x2 - x1;
            var dy = y2 - y1;
            var lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= float.Epsilon)
            {
                return MathF.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));
            }

            var t = Math.Clamp(((px - x1) * dx + (py - y1) * dy) / lengthSquared, 0f, 1f);
            var cx = x1 + t * dx - px;
            var cy = y1 + t * dy - py;
            return MathF.Sqrt(cx * cx + cy * cy);
        }

        private static bool Contains(ReadOnlySpan<(float X, float Y)> points, float px, float py)
        {
            var inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                var (xi, yi) = points[i];
                var (xj, yj) = points[j];
                if (yi > py != yj > py && px < (xj - xi) * (py - yi) / (yj - yi) + xi)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
