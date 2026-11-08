//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.PathFinder.Interfaces;

namespace NosCore.PathFinder.Brushfire
{
    /// <summary>
    /// The movement rule shared by every search: one step to any of the 8 neighbours, where a diagonal step
    /// needs at least one of the two orthogonal cells it passes to be walkable (cutting around a single wall
    /// corner is fine, squeezing between two walls that touch by a corner is not).
    /// </summary>
    internal static class GridMoves
    {
        public const int Count = 8;

        public static readonly int[] Dx = { -1, 0, 1, -1, 1, -1, 0, 1 };

        public static readonly int[] Dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        public static bool IsWalkable(IMapGrid grid, int x, int y)
        {
            return (uint)x < (uint)grid.Width && (uint)y < (uint)grid.Height && grid.IsWalkable((short)x, (short)y);
        }

        public static bool CanStep(IMapGrid grid, int x, int y, int dx, int dy)
        {
            if (!IsWalkable(grid, x + dx, y + dy))
            {
                return false;
            }

            return dx == 0 || dy == 0 || IsWalkable(grid, x + dx, y) || IsWalkable(grid, x, y + dy);
        }
    }
}
