//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Brushfire
{
    public readonly struct BrushFire
    {
        public BrushFire((short X, short Y) origin, Dictionary<(short X, short Y), double> distances, short width,
            short length)
        {
            Origin = origin;
            Distances = distances;
            Length = length;
            Width = width;
        }

        public (short X, short Y) Origin { get; }

        public Dictionary<(short X, short Y), double> Distances { get; }

        public short Length { get; }

        public short Width { get; }

        public double? this[short x, short y] => Distances.TryGetValue((x, y), out var d) ? d : null;
    }
}
