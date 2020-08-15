//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Mapster;
using NosCore.PathFinder.Gui.Database;
using NosCore.PathFinder.Heuristic;

namespace NosCore.PathFinder.Gui.Models
{
    public class MapMonsterDto : IMovableEntity
    {
        public short PositionX { get; set; }

        public short PositionY { get; set; }
        public int Speed { get; set; }

        public short MapId { get; set; }

        public int MapMonsterId { get; set; }

        public DateTime LastMove { get; set; }
        public short MapX { get; set; }

        public short MapY { get; set; }

        [AdaptIgnore]
        public IDisposable? Life { get; set; }
        public MapDto Map
        {
            get;
            set;
        } = null!;

        public void StartLife()
        {
            Life = Observable.Interval(TimeSpan.FromMilliseconds(400)).Select(_ => this.MoveAsync(new OctileDistanceHeuristic())).Subscribe();
        }
    }
}
