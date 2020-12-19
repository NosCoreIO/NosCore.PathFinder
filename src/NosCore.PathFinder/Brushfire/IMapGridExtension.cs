//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

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
                return currentX >= 0 && currentX < grid.Width &&
                        currentY >= 0 && currentY < grid.Height &&
                        (includeWalls || grid.IsWalkable(currentX, currentY)
                        );
            }).Select(delta => ((short)(cell.X + delta.X), (short)(cell.Y + delta.Y)));
        }

        public static BrushFire LoadBrushFire(this IMapGrid mapGrid, (short X, short Y) user, IHeuristic heuristic, short maxDistance = 22)
        {
            if (user.X < 0 || user.X >= mapGrid.Width ||
                user.Y < 0 || user.Y >= mapGrid.Height)
            {
                return new BrushFire(user, new Dictionary<(short X, short Y), Node?>(), mapGrid.Width, mapGrid.Height);
            }

            var path = new MinHeap();
            var cellGrid = new Dictionary<(short X, short Y), Node?>();
            var grid = new Node?[mapGrid.Width, mapGrid.Height];
            grid[user.X, user.Y] = new Node(user, mapGrid[user.X, user.Y])
            {
                Closed = true
            };
            path.Push(grid[user.X, user.Y]!);
            cellGrid[user] = new Node(user, null);

            // while the open list is not empty
            while (path.Count > 0)
            {
                // pop the position of Cell which has the minimum `f` value.
                var cell = path.Pop();
                cellGrid[cell.Position] ??= new Node(cell.Position, mapGrid[cell.Position.X, cell.Position.Y]);
                grid[cell.Position.X, cell.Position.Y] ??= new Node(cell.Position, mapGrid[cell.Position.X, cell.Position.Y]);
                grid[cell.Position.X, cell.Position.Y]!.Closed = true;

                // get neigbours of the current Cell if the neighbor has not been inspected yet, or can be reached with
                var neighbors = mapGrid.GetNeighbors(cell.Position).Select(s => grid[s.X, s.Y] ?? new Node(s, mapGrid[s.X, s.Y])).Where(neighbor => !neighbor.Closed).ToList();

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

                        cellGrid[neighbors[i]!.Position] = new Node(neighbors[i]!.Position, distance);
                        neighbors[i]!.F = distance;
                        grid[neighbors[i]!.Position.X, neighbors[i]!.Position.Y] = neighbors[i];
                    }

                    path.Push(neighbors[i]!);
                    neighbors[i]!.Closed = true;
                }
            }
            return new BrushFire(user, cellGrid, mapGrid.Width, mapGrid.Height);
        }

    }
}
