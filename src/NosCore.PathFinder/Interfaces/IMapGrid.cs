//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.PathFinder.Interfaces
{
    public interface IMapGrid
    {
        public short Width { get; }
        public short Length { get; }
        public byte this[short x, short y] { get; }
        bool IsWalkable(short currentX, short currentY);
    }
}