using System;
using System.Collections.Generic;
using System.Text;
using NosCore.PathFinder.Infrastructure;

namespace NosCore.PathFinder.Interfaces
{
    public interface IHeuristic
    {
        public double GetDistance(Cell from, Cell to);
    }
}
