using System;
using System.Threading;
using System.Threading.Tasks;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Heuristic;
using NosCore.Shared.Enumerations;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class MapNpcGo : MapNpcDto, IMovableEntity
    {
        public long VisualId => MapNpcId;

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public DateTime NextMove { get; set; }

        public MapDto Map
        {
            get;
            set;
        } = null!;

        public long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }

        public async Task StartLife(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var secondsWalking = await this.MoveAsync(new OctileDistanceHeuristic());
                await Task.Delay(secondsWalking < IMovableEntity.RefreshRate ? IMovableEntity.RefreshRate - secondsWalking : secondsWalking, cancellationToken);
            }
        }
    }
}
