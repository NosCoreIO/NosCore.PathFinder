//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder.Brushfire
{
    public class BrushFireNode : IComparable<BrushFireNode>
    {
        public BrushFireNode((short X, short Y) position, double? value)
        {
            Value = value;
            Position = position;
        }

        public (short X, short Y) Position { get; set; }

        public BrushFireNode? Parent { get; set; }

        public double? Value { get; set; }

        public double F { get; set; }

        public bool Opened { get; set; }

        public bool Closed { get; set; }

        public int CompareTo(BrushFireNode other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }
    }
}