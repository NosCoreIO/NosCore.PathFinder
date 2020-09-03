//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.PathFinder.Brushfire;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.Shared.Enumerations;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class CharacterGo : IAliveEntity
    {
        public long VisualId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int Speed { get; set; }

        public long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }

        public BrushFire? BrushFire { get; set; }

        public short PositionX
        {
            get => MapX;
            set => MapX = value;
        }

        public short PositionY
        {
            get => MapY;
            set => MapY = value;
        }

        public MapDto Map { get; set; }
    }
}
