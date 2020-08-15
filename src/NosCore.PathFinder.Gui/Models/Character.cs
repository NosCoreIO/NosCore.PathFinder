//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using NosCore.PathFinder.Gui.Database;
using NosCore.PathFinder.Infrastructure;

namespace NosCore.PathFinder.Gui.Models
{
    public class Character
    {
        public short MapX { get; set; }

        public short MapY { get; set; }

        public BrushFire BrushFire { get; set; }
    }
}
