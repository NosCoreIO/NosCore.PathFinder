//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Brushfire
{
    public readonly struct FlowField
    {
        public FlowField((short X, short Y) origin, Dictionary<(short X, short Y), (float X, float Y)> vectors, short width, short height)
        {
            Origin = origin;
            Vectors = vectors;
            Width = width;
            Height = height;
        }

        public (short X, short Y) Origin { get; }

        public Dictionary<(short X, short Y), (float X, float Y)> Vectors { get; }

        public short Width { get; }

        public short Height { get; }

        public (float X, float Y)? this[short x, short y] => Vectors.TryGetValue((x, y), out var vector) ? vector : null;
    }
}
