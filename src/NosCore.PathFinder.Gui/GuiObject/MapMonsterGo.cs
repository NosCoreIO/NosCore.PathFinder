//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using NosCore.PathFinder.Gui.Dtos;
using NosCore.PathFinder.Heuristic;
using NosCore.Shared.Enumerations;
using Serilog;

namespace NosCore.PathFinder.Gui.GuiObject
{
    public class MapMonsterGo : MapMonsterDto, IMovableEntity
    {
        private static readonly ILogger Logger = Shared.I18N.Logger.GetLoggerConfiguration().CreateLogger();
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
                try
                {
                    var secondsWalking = await this.MoveAsync(new OctileDistanceHeuristic());
                    await Task.Delay(
                        secondsWalking < IMovableEntity.RefreshRate
                            ? IMovableEntity.RefreshRate - secondsWalking
                            : secondsWalking, cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, ex.Message);
                }
            }
        }
    }
}
