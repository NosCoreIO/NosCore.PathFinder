//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Heuristic
{
    public class OctileDistanceHeuristic : IHeuristic
    {
        public readonly double Sqrt2 = Math.Sqrt(2);

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