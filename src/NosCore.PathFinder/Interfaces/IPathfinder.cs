//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using NosCore.PathFinder.Infrastructure;

namespace NosCore.PathFinder.Interfaces
{
    public interface IPathfinder
    {
        IEnumerable<Cell> FindPath(Cell start, Cell end);
    }
}