//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Reactive.Linq;
using Mapster;
using NosCore.PathFinder.Heuristic;

namespace NosCore.PathFinder.Gui.Models
{
    public class MapNpcDto
    {
        public int Speed { get; set; }

        public short MapId { get; set; }

        public int MapNpcId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }
    }
}
