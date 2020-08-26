using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Heuristic;
using NosCore.Shared.Enumerations;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class MapMonsterGo : MapMonsterDto ,IMovableEntity
    {
        public long VisualId => MapMonsterId;
        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public DateTime LastMove { get; set; }

        public IDisposable? Life { get; set; }

        public long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }

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
