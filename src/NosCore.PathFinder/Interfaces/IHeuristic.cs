namespace NosCore.PathFinder.Interfaces
{
    public interface IHeuristic
    {
        public double GetDistance((short X, short Y) from, (short X, short Y) to);
    }
}
