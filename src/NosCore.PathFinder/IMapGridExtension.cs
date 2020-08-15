using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder
{
    public static class IMapGridExtension
    {
        private static readonly List<MapCell> Neighbours = new List<MapCell> {
            new MapCell(-1, -1),  new MapCell(0, -1),  new MapCell(1, -1),
            new MapCell(-1, 0), new MapCell(1, 0),
            new MapCell(-1, 1),  new MapCell(0, 1),  new MapCell(1, 1)
        };

        public static List<Node> GetNeighbors(this IMapGrid grid, Node[,] nodeGrid, Node node)
        {
            var freeNeighbors = new List<Node>();

            foreach (var delta in Neighbours)
            {
                var currentX = (short)(node.X + delta.X);
                var currentY = (short)(node.Y + delta.Y);
                if (currentX < 0 || currentX >= grid.XLength ||
                    currentY < 0 || currentY >= grid.YLength ||
                    !grid.IsWalkable(currentX, currentY))
                {
                    continue;
                }

                freeNeighbors.Add(nodeGrid[currentX, currentY] = nodeGrid[currentX, currentY] ?? new Node(currentX, currentY, grid[currentX, currentY]));
            }

            return freeNeighbors;
        }

        public static Node[,] LoadBrushFire(this IMapGrid mapGrid, MapCell user, IHeuristic heuristic, short maxDistance = 22)
        {
            if (user.X < 0 || user.X >= mapGrid.XLength ||
                user.Y < 0 || user.Y >= mapGrid.YLength)
            {
                return new Node[mapGrid.XLength, mapGrid.YLength];
            }

            var path = new MinHeap();
            var grid = new Node[mapGrid.XLength, mapGrid.YLength];
            grid[user.X, user.Y] = new Node(user.X, user.Y, mapGrid[user.X, user.Y])
            {
                Opened = true
            };
            path.Push(grid[user.X, user.Y]);


            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of node which has the minimum `f` value.
                var node = path.Pop();
                grid[node.X, node.Y] ??= new Node(node.X, node.Y, mapGrid[node.X, node.Y]);
                grid[node.X, node.Y].Closed = true;

                // get neigbours of the current node if the neighbor has not been inspected yet, or can be reached with
                var neighbors = mapGrid.GetNeighbors(grid, node).Where(neighbor => !neighbor.Closed && !neighbor.Opened).ToList();

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    if (Equals(neighbors[i].F, 0d))
                    {
                        var distance = heuristic.GetDistance(neighbors[i], node) + node.F;
                        if (distance > maxDistance)
                        {
                            //too far count as a wall
                            neighbors[i].Value = 1;
                            continue;
                        }

                        neighbors[i].F = distance;
                        grid[neighbors[i].X, neighbors[i].Y].F = neighbors[i].F;
                    }

                    neighbors[i].Parent = node;
                    path.Push(neighbors[i]);
                    neighbors[i].Opened = true;
                }
            }
            return grid;
        }

    }
}
