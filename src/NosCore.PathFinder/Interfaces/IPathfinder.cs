﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Interfaces
{
    public interface IPathfinder
    {
        IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end);
    }
}