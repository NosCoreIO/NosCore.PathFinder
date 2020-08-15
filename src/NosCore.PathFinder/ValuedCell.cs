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
            X = x;
            Y = y;
            Value = value;
        }

        public short X { get; set; }

        public short Y { get; set; }

        public double Value { get; set; }
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