//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Gui.GuiObject;
using NosCore.PathFinder.Interfaces;
using NosCore.Shared.Helpers;

namespace NosCore.PathFinder.Gui.Dtos
{
    public class MapDto : IMapGrid
    {
        public byte[] Data { get; set; } = null!;

        public short MapId { get; set; }

        private short _xLength;

        private short _yLength;

        public short Width
        {
            get
            {
                if (_xLength == 0)
                {
                    _xLength = BitConverter.ToInt16(Data.AsSpan().Slice(0, 2).ToArray(), 0);
                }

                return _xLength;
            }
        }

        public short Length
        {
            get
            {
                if (_yLength == 0)
                {
                    _yLength = BitConverter.ToInt16(Data.AsSpan().Slice(2, 2).ToArray(), 0);
                }

                return _yLength;
            }
        }

        public ConcurrentDictionary<long, CharacterGo> Players { get; set; } =
            new ConcurrentDictionary<long, CharacterGo>();

        public byte this[short x, short y] => Data.AsSpan().Slice(4 + y * Width + x, 1)[0];

        public bool IsWalkable(short mapX, short mapY)
        {
            if ((mapX >= Width) || (mapX < 0) || (mapY >= Length) || (mapY < 0))
            {
                return false;
            }

            return IsWalkable(this[mapX, mapY]);
        }

        private static bool IsWalkable(byte value)
        {
            return (value == 0) || (value == 2) || ((value >= 16) && (value <= 19));
        }
    }
}
