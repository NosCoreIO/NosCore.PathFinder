//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.PathFinder.Interfaces
{
    public interface IHeuristic
    {
        public double GetDistance((short X, short Y) from, (short X, short Y) to);
    }
}
