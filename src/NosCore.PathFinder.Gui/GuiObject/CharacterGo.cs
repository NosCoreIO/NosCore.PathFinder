//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.PathFinder.Brushfire;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class CharacterGo
    {
        public short MapX { get; set; }

        public short MapY { get; set; }

        public BrushFire? BrushFire { get; set; }
    }
}
