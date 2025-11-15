//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Heuristic
{
    /// <summary>
    /// Heuristic that calculates octile distance (diagonal distance allowing 8-directional movement).
    /// This heuristic is optimal for grids that allow diagonal movement at sqrt(2) cost.
    /// </summary>
    public class OctileDistanceHeuristic : IHeuristic
    {
        /// <summary>
        /// The square root of 2, used as the cost multiplier for diagonal movement.
        /// </summary>
        public readonly double Sqrt2 = Math.Sqrt(2);

        /// <inheritdoc />
        public double GetDistance((short X, short Y) fromValuedCell, (short X, short Y) toValuedCell)
        {
            var iDx = Math.Abs(fromValuedCell.X - toValuedCell.X);
            var iDy = Math.Abs(fromValuedCell.Y - toValuedCell.Y);
            var min = Math.Min(iDx, iDy);
            var max = Math.Max(iDx, iDy);
            return min * Sqrt2 + max - min;
        }
    }
}