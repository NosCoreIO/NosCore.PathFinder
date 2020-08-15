//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder
{
    public class Node : IComparable<Node>
    {
        public Node(short x, short y, byte value)
        {
            ValuedCell = new ValuedCell
            {
                Value = value,
                X = x,
                Y = y,
            };
        }
        public ValuedCell ValuedCell { get; set; }

        public double F { get; internal set; }

        public bool Opened { get; internal set; }

        public bool Closed { get; internal set; }

        public int CompareTo(Node other)
        {
            return F > other.F ? 1 : F < other.F ? -1 : 0;
        }
    }
}