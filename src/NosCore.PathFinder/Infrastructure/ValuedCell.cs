//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Runtime.InteropServices;

namespace NosCore.PathFinder.Infrastructure
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public readonly struct ValuedCell
    {
        public ValuedCell(short x, short y, double value = 0)
        {
            _cell = new Cell(x, y);
            Value = value;
        }

        private readonly Cell _cell;
        public short X => _cell.X;

        public short Y => _cell.Y;

        public double Value { get; }

        public static implicit operator Cell(ValuedCell value)
        {
            return value._cell;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public readonly struct Cell
    {
        public Cell(short x, short y)
        {
            X = x;
            Y = y;
        }

        public short X { get; }

        public short Y { get; }
    }
}