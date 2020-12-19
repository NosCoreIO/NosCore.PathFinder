//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Linq;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Tests
{
    public class TestMap : IMapGrid
    {
        public TestMap(byte[][] data)
        {
            _data = data;
        }

        public short Width => (short)(_data.First().Length);
        public short Height => (short)(_data.Length);

        private readonly byte[][] _data;

        public byte this[short x, short y] => _data[y][x];

        public bool IsWalkable(short currentX, short currentY)
        {
            return _data[currentY][currentX] == 0;
        }
    }
}