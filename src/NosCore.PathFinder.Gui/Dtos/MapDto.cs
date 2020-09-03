﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
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

        public short XLength
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

        public short YLength
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

        public List<CharacterGo> Players { get; set; }

        public byte this[short x, short y] => Data.AsSpan().Slice(4 + y * XLength + x, 1)[0];

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            var minX = (short)(-xpoint + firstX);
            var maxX = (short)(xpoint + firstX);

            var minY = (short)(-ypoint + firstY);
            var maxY = (short)(ypoint + firstY);

            var cells = new List<(short X, short Y)>();
            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    if ((x != firstX) || (y != firstY))
                    {
                        cells.Add((x, y));
                    }
                }
            }

            foreach (var cell in cells.OrderBy(_ => RandomHelper.Instance.RandomNumber(0, int.MaxValue)))
            {
                if (IsBlockedZone(firstX, firstY, cell.X, cell.Y))
                {
                    continue;
                }

                firstX = cell.X;
                firstY = cell.Y;
                return true;
            }

            return false;
        }

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
            if ((mapX > XLength) || (mapX < 0) || (mapY > YLength) || (mapY < 0))
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
