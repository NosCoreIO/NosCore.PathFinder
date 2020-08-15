using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder
{
    public static class IMapGridExtension
    {
        private static readonly List<ValuedCell> Neighbours = new List<ValuedCell> {
            new ValuedCell(-1, -1),  new ValuedCell(0, -1),  new ValuedCell(1, -1),
            new ValuedCell(-1, 0), new ValuedCell(1, 0),
            new ValuedCell(-1, 1),  new ValuedCell(0, 1),  new ValuedCell(1, 1)
        };

        public static List<Cell> GetNeighbors(this IMapGrid grid, ValuedCell valuedCell)
        {
            var freeNeighbors = new List<Cell>();

            foreach (var delta in Neighbours)
            {
                var currentX = (short)(valuedCell.X + delta.X);
                var currentY = (short)(valuedCell.Y + delta.Y);
                if (currentX < 0 || currentX >= grid.XLength ||
                    currentY < 0 || currentY >= grid.YLength ||
                    !grid.IsWalkable(currentX, currentY))
                {
                    continue;
                }

                freeNeighbors.Add(new Cell(currentX, currentY));
            }

            return freeNeighbors;
        }

        public static ValuedCell?[,] LoadBrushFire(this IMapGrid mapGrid, Cell user, IHeuristic heuristic, short maxDistance = 22)
        {
            if (user.X < 0 || user.X >= mapGrid.XLength ||
                user.Y < 0 || user.Y >= mapGrid.YLength)
            {
                return new ValuedCell?[mapGrid.XLength, mapGrid.YLength];
            }

            var path = new MinHeap();
            var cellGrid = new ValuedCell?[mapGrid.XLength, mapGrid.YLength];
            var grid = new Node[mapGrid.XLength, mapGrid.YLength];
            grid[user.X, user.Y] = new Node(user.X, user.Y, mapGrid[user.X, user.Y])
            {
                Opened = true
            };
            path.Push(grid[user.X, user.Y]);
            cellGrid[user.X, user.Y] = new ValuedCell(user.X, user.Y);

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of Cell which has the minimum `f` value.
                var cell = path.Pop();
                cellGrid[cell.ValuedCell.X, cell.ValuedCell.Y] ??= new ValuedCell(cell.ValuedCell.X, cell.ValuedCell.Y, mapGrid[cell.ValuedCell.X, cell.ValuedCell.Y]);
                grid[cell.ValuedCell.X, cell.ValuedCell.Y] ??= new Node(cell.ValuedCell.X, cell.ValuedCell.Y, mapGrid[cell.ValuedCell.X, cell.ValuedCell.Y]);
                grid[cell.ValuedCell.X, cell.ValuedCell.Y].Closed = true;

                // get neigbours of the current Cell if the neighbor has not been inspected yet, or can be reached with
                var neighbors = mapGrid.GetNeighbors(cell.ValuedCell).Select(s => grid[s.X, s.Y] ?? new Node(s.X, s.Y, mapGrid[s.X, s.Y])).Where(neighbor => !neighbor.Closed && !neighbor.Opened).ToList();

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    if (Equals(neighbors[i].F, 0d))
                    {
                        var distance = heuristic.GetDistance(neighbors[i].ValuedCell, cell.ValuedCell) + cell.F;
                        if (distance > maxDistance)
                        {
                            //too far count as a wall
                            neighbors[i].ValuedCell = new ValuedCell(neighbors[i].ValuedCell.X, neighbors[i].ValuedCell.Y, 1);
                            continue;
                        }

                        cellGrid[neighbors[i].ValuedCell.X, neighbors[i].ValuedCell.Y] = new ValuedCell(neighbors[i].ValuedCell.X, neighbors[i].ValuedCell.Y, distance);
                        neighbors[i].F = distance;
                        grid[neighbors[i].ValuedCell.X, neighbors[i].ValuedCell.Y] = neighbors[i];
                    }

                    path.Push(neighbors[i]);
                    neighbors[i].Opened = true;
                }
            }
            return cellGrid;
        }

    }
}
