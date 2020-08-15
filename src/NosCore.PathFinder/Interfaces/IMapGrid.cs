namespace NosCore.PathFinder.Interfaces
{
    public interface IMapGrid
    {
        public short XLength { get; }
        public short YLength { get; }
        public byte this[short x, short y] { get; }
        bool IsWalkable(short currentX, short currentY);
    }
}