using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using NosCore.PathFinder.Gui.Models;
using NosCore.PathFinder.Heuristic;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class MapMonsterGo : MapMonsterDto ,IMovableEntity
    {
        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public DateTime LastMove { get; set; }

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
