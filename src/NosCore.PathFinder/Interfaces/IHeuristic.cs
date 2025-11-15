//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.PathFinder.Interfaces
{
    /// <summary>
    /// Interface for heuristic distance calculation used in pathfinding algorithms.
    /// </summary>
    public interface IHeuristic
    {
        /// <summary>
        /// Calculates the estimated distance between two points.
        /// </summary>
        /// <param name="from">The starting point coordinates.</param>
        /// <param name="to">The ending point coordinates.</param>
        /// <returns>The estimated distance between the two points.</returns>
        public double GetDistance((short X, short Y) from, (short X, short Y) to);
    }
}
