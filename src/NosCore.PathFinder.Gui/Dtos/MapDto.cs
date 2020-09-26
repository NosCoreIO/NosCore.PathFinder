//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        internal (short X, short Y)? GetFreePosition(short firstX, short firstY, byte xradius, byte yradius)
        {
            IEnumerable<(short X, short Y)?> GetCellsInRadius()
            {
                for (var y = -yradius; y <= yradius; y++)
                {
                    var projectedY = (short) Math.Clamp(y + firstY, 0, short.MaxValue);
                    for (var x = -xradius; x <= xradius; x++)
                    {
                        if ((x != firstX) || (y != firstY))
                        {
                            yield return ((short)Math.Clamp(x + firstX, 0, short.MaxValue), projectedY);
                        }
                    }
                }
            }

            return GetCellsInRadius().OrderBy(_ => RandomHelper.Instance.RandomNumber(0, int.MaxValue))
                .FirstOrDefault(c => !IsBlockedZone(firstX, firstY, c!.Value.X, c.Value.Y));
        }

        //todo fix that stupid method
        public bool IsBlockedZone(short firstX, short firstY, short mapX, short mapY)
        {
            var posX = (short)Math.Abs(mapX - firstX);
            var posY = (short)Math.Abs(mapY - firstY);

            var positiveX = mapX > firstX;
            var positiveY = mapY > firstY;

            for (var i = 0; i <= posX; i++)
            {
                if (!IsWalkable((short)((positiveX ? i : -i) + firstX), firstY))
                {
                    return true;
                }
            }

            for (var i = 0; i <= posY; i++)
            {
                if (!IsWalkable(firstX, (short)((positiveY ? i : -i) + firstY)))
                {
                    return true;
                }
            }

            return false;
        }

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
