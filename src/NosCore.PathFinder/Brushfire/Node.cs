//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder.Brushfire
{
    public class Node : IComparable<Node>
    {
        public Node((short X, short Y) position, double? value)
        {
            Value = value;
            Position = position;
        }

        public Node()
        {
        }

        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }
        public bool Closed { get; set; }

        public Node? Parent { get; set; }

        public double F { get; set; }

        public (short X, short Y) Position { get; set; }

        public double? Value { get; set; }
    }
}