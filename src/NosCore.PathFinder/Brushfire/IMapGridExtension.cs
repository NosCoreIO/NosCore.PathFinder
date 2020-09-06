using System.Collections.Generic;
using System.Linq;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Brushfire
{
    public static class IMapGridExtension
    {
        private static readonly List<(short X, short Y)> Neighbours = new List<(short, short)> {
            (-1, -1),  (0, -1),  (1, -1),
            (-1, 0), (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

        public static IEnumerable<(short X, short Y)> GetNeighbors(this IMapGrid grid, (short X, short Y) cell, bool includeWalls = false)
        {
            return Neighbours.Where(delta =>
            {
                var currentX = (short)(cell.X + delta.X);
                var currentY = (short)(cell.Y + delta.Y);
                return currentX >= 0 && currentX < grid.XLength &&
                        currentY >= 0 && currentY < grid.YLength &&
                        (includeWalls || grid.IsWalkable(currentX, currentY)
                        );
            }).Select(delta => ((short)(cell.X + delta.X), (short)(cell.Y + delta.Y)));
        }

        public static BrushFire LoadBrushFire(this IMapGrid mapGrid, (short X, short Y) user, IHeuristic heuristic, short maxDistance = 22)
        {
            if (user.X < 0 || user.X >= mapGrid.XLength ||
                user.Y < 0 || user.Y >= mapGrid.YLength)
            {
                return new BrushFire(user, new Dictionary<(short X, short Y), BrushFireNode?>(), mapGrid.XLength, mapGrid.XLength);
            }

            var path = new MinHeap();
            var cellGrid = new Dictionary<(short X, short Y), BrushFireNode?>();
            var grid = new BrushFireNode?[mapGrid.XLength, mapGrid.YLength];
            grid[user.X, user.Y] = new BrushFireNode(user, mapGrid[user.X, user.Y])
            {
                Opened = true
            };
            path.Push(grid[user.X, user.Y]!);
            cellGrid[user] = new BrushFireNode(user, null);

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of Cell which has the minimum `f` value.
                var cell = path.Pop();
                cellGrid[cell.Position] ??= new BrushFireNode(cell.Position, mapGrid[cell.Position.X, cell.Position.Y]);
                grid[cell.Position.X, cell.Position.Y] ??= new BrushFireNode(cell.Position, mapGrid[cell.Position.X, cell.Position.Y]);
                grid[cell.Position.X, cell.Position.Y]!.Closed = true;

                // get neigbours of the current Cell if the neighbor has not been inspected yet, or can be reached with
                var neighbors = mapGrid.GetNeighbors(cell.Position).Select(s => grid[s.X, s.Y] ?? new BrushFireNode(s, mapGrid[s.X, s.Y])).Where(neighbor => !neighbor.Closed && !neighbor.Opened).ToList();

                for (int i = 0, l = neighbors.Count; i < l; ++i)
                {
                    if (Equals(neighbors[i]!.F, 0d))
                    {
                        var distance = heuristic.GetDistance(neighbors[i]!.Position, cell.Position) + cell.F;
                        if (distance > maxDistance)
                        {
                            //too far count as a wall
                            neighbors[i]!.Value = null;
                            continue;
                        }

                        cellGrid[neighbors[i]!.Position] = new BrushFireNode(neighbors[i]!.Position, distance);
                        neighbors[i]!.F = distance;
                        grid[neighbors[i]!.Position.X, neighbors[i]!.Position.Y] = neighbors[i];
                    }

                    path.Push(neighbors[i]!);
                    neighbors[i]!.Opened = true;
                }
            }
            return new BrushFire(user, cellGrid, mapGrid.XLength, mapGrid.XLength);
        }

    }
}
