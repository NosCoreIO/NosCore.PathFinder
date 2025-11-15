//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Brushfire
{
    /// <summary>
    /// Represents a brushfire pathfinding data structure containing pre-computed distance information.
    /// </summary>
    public readonly struct BrushFire
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BrushFire"/> struct.
        /// </summary>
        /// <param name="origin">The origin point of the brushfire.</param>
        /// <param name="brushFireGrid">The grid containing computed node information.</param>
        /// <param name="width">The width of the grid.</param>
        /// <param name="length">The length of the grid.</param>
        public BrushFire((short X, short Y) origin, Dictionary<(short X, short Y), Node?> brushFireGrid, short width,
            short length)
        {
            Origin = origin;
            Grid = brushFireGrid;
            Length = length;
            Width = width;
        }

        /// <summary>
        /// Gets the origin point of the brushfire.
        /// </summary>
        public (short X, short Y) Origin { get; }

        /// <summary>
        /// Gets the grid containing computed node information.
        /// </summary>
        public Dictionary<(short X, short Y), Node?> Grid { get; }

        /// <summary>
        /// Gets the length of the grid.
        /// </summary>
        public short Length { get; }

        /// <summary>
        /// Gets the width of the grid.
        /// </summary>
        public short Width { get; }

        /// <summary>
        /// Gets the value at the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The value at the specified position, or null if not found.</returns>
        public double? this[short x, short y] => Grid.ContainsKey((x, y)) ? Grid[(x, y)]?.Value : null;
    }
}
