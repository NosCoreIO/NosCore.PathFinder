//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace NosCore.PathFinder
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ValuedCell
    {
        public ValuedCell(short x, short y, double value = 0)
        {
            _cell = new Cell(x, y);
            Value = value;
        }

        private readonly Cell _cell;
        public short X => _cell.X;

        public short Y => _cell.Y;

        public double Value { get; set; }

        public static implicit operator Cell(ValuedCell value)
        {
            return value._cell;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Cell
    {
        public Cell(short x, short y)
        {
            X = x;
            Y = y;
        }

        public short X { get; set; }

        public short Y { get; set; }
    }
}