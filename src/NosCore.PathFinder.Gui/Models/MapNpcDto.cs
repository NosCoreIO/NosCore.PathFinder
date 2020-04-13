using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NosCore.PathFinder.Gui.Database;

namespace NosCore.PathFinder.Gui.Models
{
    public class MapNpcDto : IMovableEntity
    {
        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public DateTime LastMove { get; set; }

        public int Speed { get; set; }

        public short MapId { get; set; }

        public int MapNpcId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }
        public MapDto Map { get; set; } = null!;

        public IDisposable? Life { get; private set; }

        public void StartLife()
        {
            Life = Observable.Interval(TimeSpan.FromMilliseconds(400)).Select(_ => this.MoveAsync(new OctileDistanceCalculator())).Subscribe();
        }
    }
}
