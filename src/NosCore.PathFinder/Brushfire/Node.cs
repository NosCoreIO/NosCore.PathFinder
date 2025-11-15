//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder.Brushfire
{
    /// <summary>
    /// Represents a node in the pathfinding grid with position and value information.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="position">The position of the node in the grid.</param>
        /// <param name="value">The value associated with the node.</param>
        public Node((short X, short Y) position, double? value)
        {
            Value = value;
            Position = position;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this node has been processed and closed.
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// Gets or sets the parent node in the path.
        /// </summary>
        public Node? Parent { get; set; }

        /// <summary>
        /// Gets or sets the F score (G + H) used in A* pathfinding.
        /// </summary>
        public double F { get; set; }

        /// <summary>
        /// Gets or sets the position of the node in the grid.
        /// </summary>
        public (short X, short Y) Position { get; set; }

        /// <summary>
        /// Gets or sets the value associated with this node.
        /// </summary>
        public double? Value { get; set; }
    }
}