//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Interfaces
{
    /// <summary>
    /// Interface for pathfinding algorithms that find paths on a grid-based map.
    /// </summary>
    public interface IPathfinder
    {
        /// <summary>
        /// Finds a path between two points on the map grid.
        /// </summary>
        /// <param name="start">The starting coordinates (X, Y).</param>
        /// <param name="end">The ending coordinates (X, Y).</param>
        /// <returns>An enumerable sequence of coordinates representing the path from start to end.</returns>
        IEnumerable<(short X, short Y)> FindPath((short X, short Y) start, (short X, short Y) end);
    }
}