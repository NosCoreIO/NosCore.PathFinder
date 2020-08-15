//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder
{
    public class OctileDistanceHeuristic : IHeuristic
    {
        public readonly double Sqrt2 = Math.Sqrt(2);

        public double GetDistance(Cell fromCell, Cell toCell)
        {
            var iDx = Math.Abs(fromCell.X - toCell.X);
            var iDy = Math.Abs(fromCell.Y - toCell.Y);
            var min = Math.Min(iDx, iDy);
            var max = Math.Max(iDx, iDy);
            return min * Sqrt2 + max - min;
        }
    }
}