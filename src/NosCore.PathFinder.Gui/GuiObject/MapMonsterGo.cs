using System;
using System.Threading;
using System.Threading.Tasks;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Heuristic;
using NosCore.Shared.Enumerations;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class MapMonsterGo : MapMonsterDto, IMovableEntity
    {
        public long VisualId => MapMonsterId;
        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public DateTime NextMove { get; set; }

        public long? TargetVisualId { get; set; }

        public VisualType? TargetVisualType { get; set; }

        public MapDto Map { get; set; } = null!;

        public async Task StartLife(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var secondsWalking = await this.MoveAsync(new OctileDistanceHeuristic());
                await Task.Delay(secondsWalking < 400 ? 400 - secondsWalking : secondsWalking, cancellationToken);
            }
        }
    }
}
