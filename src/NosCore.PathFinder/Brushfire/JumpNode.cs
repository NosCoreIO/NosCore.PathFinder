//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;

namespace NosCore.PathFinder.Brushfire
{
    /// <summary>
    /// Represents a node used in Jump Point Search pathfinding algorithm.
    /// Extends the base Node class with additional properties for JPS.
    /// </summary>
    public class JumpNode : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JumpNode"/> class.
        /// </summary>
        /// <param name="position">The position of the node in the grid.</param>
        /// <param name="value">The value associated with the node.</param>
        public JumpNode((short X, short Y) position, double? value) : base(position, value)
        {
        }

        /// <summary>
        /// Gets or sets the G score (cost from start to this node).
        /// </summary>
        public double G { get; set; }

        /// <summary>
        /// Gets or sets the H score (heuristic cost from this node to the goal).
        /// </summary>
        public double? H { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node has been opened for evaluation.
        /// </summary>
        public bool Opened { get; set; }
    }
}