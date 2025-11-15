//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.PathFinder.Interfaces
{
    /// <summary>
    /// Interface representing a grid-based map for pathfinding.
    /// </summary>
    public interface IMapGrid
    {
        /// <summary>
        /// Gets the width of the map grid.
        /// </summary>
        public short Width { get; }

        /// <summary>
        /// Gets the height of the map grid.
        /// </summary>
        public short Height { get; }

        /// <summary>
        /// Gets the cell value at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The byte value representing the cell at the given position.</returns>
        public byte this[short x, short y] { get; }

        /// <summary>
        /// Determines whether the specified position is walkable.
        /// </summary>
        /// <param name="currentX">The X coordinate to check.</param>
        /// <param name="currentY">The Y coordinate to check.</param>
        /// <returns>True if the position is walkable; otherwise, false.</returns>
        bool IsWalkable(short currentX, short currentY);
    }
} 