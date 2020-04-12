﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.PathFinder
{
    public class MapCell
    {
        public MapCell() { }

        public MapCell(short x, short y)
        {
            X = x;
            Y = y;
        }

        public short X { get; set; }
        public short Y { get; set; }
    }
}