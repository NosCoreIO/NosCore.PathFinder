using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder
{
    public class BestFirstSearchPathfinder : IPathfinder
    {
        private readonly IHeuristic _heuristic;
        private readonly IMapGrid _grid;

        public BestFirstSearchPathfinder(IHeuristic heuristic, IMapGrid grid)
        {
            _heuristic = heuristic;
            _grid = grid;
        }

        public IEnumerable<MapCell> FindPath(MapCell start, MapCell end)
        {
            if (_grid.XLength <= start.X || _grid.YLength <= start.Y || start.X < 0 || start.Y < 0)
            {
                return new List<MapCell>();
            }

            var nodeGrid = new Node[_grid.XLength, _grid.YLength];
            nodeGrid[start.X, start.Y] ??= new Node(start.X, start.Y, _grid[start.X, start.Y]);
            var startPos = nodeGrid[start.X, start.Y];
            var path = new MinHeap();

            // push the start node into the open list
            path.Push(startPos);
            startPos.Opened = true;

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                var node = path.Pop();
                nodeGrid[node.X, node.Y] ??= new Node(node.X, node.Y, _grid[node.X, node.Y]);
                nodeGrid[node.X, node.Y].Closed = true;

                //if reached the end position, construct the path and return it
                if (node.X == end.X && node.Y == end.Y)
                {
                    return Backtrace(node);
                }

                // get neigbours of the current node
                var neighbors = _grid.GetNeighbors(nodeGrid, node);

                for (int i = 0, l = neighbors.Count(); i < l; ++i)
                {
                    var neighbor = neighbors[i];

                    if (neighbor.Closed)
                    {
                        continue;
                    }

                    // check if the neighbor has not been inspected yet, or can be reached with
                    // smaller cost from the current node
                    if (neighbor.Opened) continue;
                    if (neighbor.F == 0)
                    {
                        neighbor.F = _heuristic.GetDistance(neighbor, end);
                    }

                    neighbor.Parent = node;

                    if (!neighbor.Opened)
                    {
                        path.Push(neighbor);
                        neighbor.Opened = true;
                    }
                    else
                    {
                        neighbor.Parent = node;
                    }
                }
            }
            return new List<MapCell>();
        }

        public static List<MapCell> Backtrace(Node end)
        {
            var path = new List<MapCell>();
            while (end.Parent != null)
            {
                end = end.Parent;
                path.Add(end);
            }
            path.Reverse();
            return path;
        }
    }
}
