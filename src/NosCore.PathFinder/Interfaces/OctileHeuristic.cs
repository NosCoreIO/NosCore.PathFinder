using System;
using System.Collections.Generic;
using System.Text;

namespace NosCore.PathFinder.Interfaces
{
    public class OctileHeuristic : IHeuristic
    {
        private static readonly double Sqrt2 = Math.Sqrt(2);
        public double GetDistance(MapCell from, MapCell to)
        {
            var iDx = Math.Abs(from.X - to.X);
            var iDy = Math.Abs(from.Y - to.Y);
            var min = Math.Min(iDx, iDy);
            var max = Math.Max(iDx, iDy);
            return min * Sqrt2 + max - min;
        }
    }
}
