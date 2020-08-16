using NosCore.PathFinder.Infrastructure;

namespace NosCore.PathFinder.Pathfinder
{
    public class GoalBasedNode
    {
        public GoalBasedNode(Cell cell, GoalBasedNode? parent)
        {
            Cell = cell;
            Parent = parent;
        }
        public Cell Cell { get; set; }
        public GoalBasedNode? Parent { get; set; }
    }
}
