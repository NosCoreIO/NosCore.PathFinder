using System;
using System.Collections.Generic;
using System.Text;
using NosCore.PathFinder.Gui.Database;

namespace NosCore.PathFinder.Gui.Models
{
    public class MapMonsterDto : MapMonster
    {
        public short PositionX { get; set; }

        public short PositionY { get; set; }
        public int Speed { get; set; }
    }
}
