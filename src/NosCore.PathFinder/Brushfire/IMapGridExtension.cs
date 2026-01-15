//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Brushfire
{
    public static class IMapGridExtension
    {
        private static readonly float Sqrt2Inv = 1f / (float)Math.Sqrt(2);

        private static readonly List<(short X, short Y)> Neighbours = new List<(short, short)> {
            (-1, -1),  (0, -1),  (1, -1),
            (-1, 0), (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

        public static IEnumerable<(short X, short Y)> GetNeighbors(this IMapGrid grid, (short X, short Y) cell, bool includeWalls = false)
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                var delta = Neighbours[i];
                var currentX = (short)(cell.X + delta.X);
                var currentY = (short)(cell.Y + delta.Y);
                if (currentX >= 0 && currentX < grid.Width &&
                    currentY >= 0 && currentY < grid.Height &&
                    (includeWalls || grid.IsWalkable(currentX, currentY)))
                {
                    yield return (currentX, currentY);
                }
            }
        }

        public static BrushFire LoadBrushFire(this IMapGrid mapGrid, (short X, short Y) user, IHeuristic heuristic, short maxDistance = 22)
        {
            if (user.X < 0 || user.X >= mapGrid.Width ||
                user.Y < 0 || user.Y >= mapGrid.Height)
            {
                return new BrushFire(user, new Dictionary<(short X, short Y), double>(), mapGrid.Width, mapGrid.Height);
            }

            var queue = new PriorityQueue<(short X, short Y, double D), double>();
            var distances = new Dictionary<(short X, short Y), double>();

            queue.Enqueue((user.X, user.Y, 0), 0);
            distances[user] = 0;

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var cellPos = (cell.X, cell.Y);

                if (distances.TryGetValue(cellPos, out var existingDist) && existingDist < cell.D)
                {
                    continue;
                }

                foreach (var neighborPos in mapGrid.GetNeighbors(cellPos))
                {
                    var distance = heuristic.GetDistance(neighborPos, cellPos) + cell.D;
                    if (distance > maxDistance)
                    {
                        continue;
                    }

                    if (!distances.TryGetValue(neighborPos, out var neighborDist) || distance < neighborDist)
                    {
                        distances[neighborPos] = distance;
                        queue.Enqueue((neighborPos.X, neighborPos.Y, distance), distance);
                    }
                }
            }
            return new BrushFire(user, distances, mapGrid.Width, mapGrid.Height);
        }

        public static FlowField GetFlowField(this BrushFire brushFire, IMapGrid mapGrid, double stopDistance = 0)
        {
            var vectors = new Dictionary<(short X, short Y), (float X, float Y)>();

            foreach (var kvp in brushFire.Distances)
            {
                var pos = kvp.Key;
                var currentDistance = kvp.Value;

                if (pos == brushFire.Origin || currentDistance <= stopDistance)
                {
                    continue;
                }

                var bestNeighbor = pos;
                var bestDistance = currentDistance;

                foreach (var neighbor in mapGrid.GetNeighbors(pos))
                {
                    if (neighbor == brushFire.Origin)
                    {
                        bestDistance = 0;
                        bestNeighbor = neighbor;
                        break;
                    }

                    if (brushFire.Distances.TryGetValue(neighbor, out var neighborDistance) && neighborDistance < bestDistance)
                    {
                        bestDistance = neighborDistance;
                        bestNeighbor = neighbor;
                    }
                }

                if (bestNeighbor != pos)
                {
                    var dx = bestNeighbor.X - pos.X;
                    var dy = bestNeighbor.Y - pos.Y;

                    if (dx != 0 && dy != 0)
                    {
                        vectors[pos] = (dx * Sqrt2Inv, dy * Sqrt2Inv);
                    }
                    else
                    {
                        vectors[pos] = (dx, dy);
                    }
                }
            }

            return new FlowField(brushFire.Origin, vectors, brushFire.Width, brushFire.Length);
        }

    }
}
